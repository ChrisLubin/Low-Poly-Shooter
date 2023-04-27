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
    private List<SoldierController> _players = new();
    private SoldierController _localPlayer;
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
        SoldierController.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
        SoldierController.OnSpawn += this.OnSpawn;
        SoldierController.OnDeath += this.OnDeath;
        SoldierController.OnShot += this.OnShot;
        SoldierController.OnDamageReceived += this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShot += this.OnServerShot;
        RpcSystem.OnPlayerDamageReceived += this.OnServerDamageReceived;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        SoldierController.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
        SoldierController.OnSpawn -= this.OnSpawn;
        SoldierController.OnDeath -= this.OnDeath;
        SoldierController.OnShot -= this.OnShot;
        SoldierController.OnDamageReceived -= this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShot -= this.OnServerShot;
        RpcSystem.OnPlayerDamageReceived -= this.OnServerDamageReceived;
    }

    private void OnLocalPlayerSpawn(SoldierController player) => this._localPlayer = player;
    private void OnSpawn(SoldierController player) => this._players.Add(player);

    private async void OnDeath(SoldierController player)
    {
        this._players.Remove(player);

        if (!this.IsHost) { return; }

        // Wait to despawn player so all clients get time to spawn ragdoll if host dies
        await Task.Delay(TimeSpan.FromSeconds(_PLAYER_DESPAWN_TIMER));
        player.GetComponent<NetworkObject>().Despawn();

    }

    private void OnShot(SoldierController player)
    {
        if (player != this._localPlayer) { return; }
        RpcSystem.Instance.OnPlayerShotServerRpc(NetworkManager.Singleton.LocalClientId, this._players.IndexOf(player));
        SoldierManager.OnLocalPlayerShot?.Invoke();
    }

    private void OnServerShot(int playerIndex)
    {
        SoldierController player = this._players.ElementAtOrDefault(playerIndex);

        if (!player)
        {
            this._logger.Log("Unable to find player who shot", Logger.LogLevel.Error);
            return;
        }
        if (player == this._localPlayer) { return; }

        player.Shoot();
    }

    private void OnLocalDamageReceived(SoldierController player, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (player == this._localPlayer) { return; }
        RpcSystem.Instance.OnPlayerDamageReceivedServerRpc(NetworkManager.Singleton.LocalClientId, this._players.IndexOf(player), damageType, damageAmount);
    }

    private void OnServerDamageReceived(int playerIndex, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        SoldierController player = this._players.ElementAtOrDefault(playerIndex);

        if (!player)
        {
            this._logger.Log("Unable to find player who took damage", Logger.LogLevel.Error);
            return;
        }
        if (player != this._localPlayer) { return; }

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
