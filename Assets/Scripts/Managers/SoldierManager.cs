using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class SoldierManager : NetworkedStaticInstanceWithLogger<SoldierManager>
{
    [SerializeField] private Transform _spawnedSoldiersParent;
    [SerializeField] private Transform _spawnPointsParent;
    [SerializeField] Transform _playerPrefab;

    private List<Transform> _spawnPoints = new();
    private IDictionary<ulong, SoldierController> _playersMap = new Dictionary<ulong, SoldierController>();
    private ulong _localClientId;
    private bool _hasSpawnedPlayers = false;
    private const int _PLAYER_DESPAWN_TIMER = 5;

    public static Action OnLocalPlayerShot;
    public static Action OnLocalPlayerDamageReceived;

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform spawnPoint in this._spawnPointsParent)
        {
            this._spawnPoints.Add(spawnPoint);
        }
        SoldierController.OnSpawn += this.OnSpawn;
        SoldierController.OnDeath += this.OnDeath;
        SoldierController.OnShot += this.OnShot;
        SoldierController.OnDamageReceived += this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShot += this.OnServerShot;
        RpcSystem.OnPlayerDamageReceived += this.OnServerDamageReceived;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        this._localClientId = NetworkManager.Singleton.LocalClientId;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        SoldierController.OnSpawn -= this.OnSpawn;
        SoldierController.OnDeath -= this.OnDeath;
        SoldierController.OnShot -= this.OnShot;
        SoldierController.OnDamageReceived -= this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShot -= this.OnServerShot;
        RpcSystem.OnPlayerDamageReceived -= this.OnServerDamageReceived;
    }

    private void OnSpawn(ulong clientId, SoldierController player) => this._playersMap[clientId] = player;

    private async void OnDeath(ulong clientId)
    {
        this._playersMap.TryGetValue(clientId, out SoldierController player);
        this._playersMap.Remove(clientId);

        if (!this.IsHost) { return; }

        // Wait to despawn player so all clients get time to spawn ragdoll if host dies
        await Task.Delay(TimeSpan.FromSeconds(_PLAYER_DESPAWN_TIMER));
        player.GetComponent<NetworkObject>().Despawn();
    }

    private void OnShot(ulong clientId)
    {
        if (clientId != this._localClientId) { return; }
        RpcSystem.Instance.OnPlayerShotServerRpc(NetworkManager.Singleton.LocalClientId, this._localClientId);
        SoldierManager.OnLocalPlayerShot?.Invoke();
    }

    private void OnServerShot(ulong clientId)
    {
        if (!this._playersMap.TryGetValue(clientId, out SoldierController player)) { return; }
        if (clientId == this._localClientId) { return; }

        player.Shoot();
    }

    private void OnLocalDamageReceived(ulong clientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (clientId == this._localClientId) { return; }
        RpcSystem.Instance.OnPlayerDamageReceivedServerRpc(NetworkManager.Singleton.LocalClientId, clientId, damageType, damageAmount);
    }

    private void OnServerDamageReceived(ulong clientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (!this._playersMap.TryGetValue(clientId, out SoldierController player)) { return; }
        if (clientId != this._localClientId) { return; }

        player.TakeServerDamage(damageType, damageAmount);
        SoldierManager.OnLocalPlayerDamageReceived?.Invoke();
    }

    public void SpawnSoldiers()
    {
        if (!this.IsHost || this._hasSpawnedPlayers) { return; }
        this._logger.Log("Spawning soldiers");

        ulong[] connectedClientIds = Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds);

        for (int i = 0; i < connectedClientIds.Count(); i++)
        {
            ulong clientId = connectedClientIds[i];
            if (i + 1 > this._spawnPoints.Count)
            {
                this._logger.Log($"Not enough spawn points to spawn player for client ID {clientId}", Logger.LogLevel.Error);
                continue;
            }
            Transform spawnPoint = this._spawnPoints[i];

            Transform playerTransform = Instantiate(this._playerPrefab, spawnPoint.position, Quaternion.identity, this._spawnedSoldiersParent);
            playerTransform.rotation = spawnPoint.rotation;
            playerTransform.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            this._logger.Log($"Spawned soldier for client ID {clientId}");
        }

        this._logger.Log("Spawned soldiers");
        this._hasSpawnedPlayers = true;
        RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameStarted);
    }
}
