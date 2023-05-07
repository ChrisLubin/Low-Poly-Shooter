using System;
using System.Collections.Generic;
using System.Linq;
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
    private const int _DEAD_PLAYER_DESPAWN_TIMER = 5000;
    public const int SPAWN_PLAYER_REQUEST_TIMER = 3000; // Make private after doing Cinemachine fix

    public static event Action OnLocalPlayerShoot;
    public static event Action OnLocalPlayerDamageReceived;
    public static event Action OnLocalPlayerSpawn;
    public static event Action OnLocalPlayerDeath;

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform spawnPoint in this._spawnPointsParent)
        {
            this._spawnPoints.Add(spawnPoint);
        }
        SoldierController.OnSpawn += this.OnSpawn;
        SoldierController.OnDeath += this.OnDeath;
        SoldierController.OnShoot += this.OnShoot;
        SoldierController.OnDamageReceived += this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShoot += this.OnServerShoot;
        RpcSystem.OnPlayerDamageReceived += this.OnServerDamageReceived;
        RpcSystem.OnPlayerRequestSpawn += this.OnPlayerRequestSpawn;
        GameManager.OnStateChange += this.OnGameStateChange;
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
        SoldierController.OnShoot -= this.OnShoot;
        SoldierController.OnDamageReceived -= this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShoot -= this.OnServerShoot;
        RpcSystem.OnPlayerDamageReceived -= this.OnServerDamageReceived;
        RpcSystem.OnPlayerRequestSpawn -= this.OnPlayerRequestSpawn;
        GameManager.OnStateChange -= this.OnGameStateChange;
    }

    private void OnGameStateChange(GameState state)
    {
        if (!this.IsHost) { return; }

        switch (state)
        {
            case GameState.GameStarting:
                this.SpawnPlayers();
                break;
        }
    }

    private void OnSpawn(ulong clientId, SoldierController player)
    {
        this._playersMap[clientId] = player;
        if (clientId == this._localClientId)
        {
            SoldierManager.OnLocalPlayerSpawn?.Invoke();
        }
    }

    private async void OnDeath(ulong clientId)
    {
        this._logger.Log($"Soldier for client ID {clientId} died");
        this._playersMap.TryGetValue(clientId, out SoldierController player);
        this._playersMap.Remove(clientId);
        if (clientId == this._localClientId)
        {
            SoldierManager.OnLocalPlayerDeath?.Invoke();
            // Request server to spawn us after a timer
            await UnityTimer.Delay(SPAWN_PLAYER_REQUEST_TIMER);
            RpcSystem.Instance.RequestPlayerSpawnServerRpc();
        }

        if (!this.IsHost) { return; }

        // Wait to despawn player so all clients get time to spawn ragdoll if host dies
        await UnityTimer.Delay(_DEAD_PLAYER_DESPAWN_TIMER);
        player.GetComponent<NetworkObject>().Despawn();
    }

    private void OnShoot(ulong clientId)
    {
        if (clientId != this._localClientId) { return; }
        RpcSystem.Instance.OnPlayerShootServerRpc();
        SoldierManager.OnLocalPlayerShoot?.Invoke();
    }

    private void OnServerShoot(ulong clientId)
    {
        if (!this._playersMap.TryGetValue(clientId, out SoldierController player)) { return; }
        if (clientId == this._localClientId) { return; }

        player.Shoot();
    }

    private void OnLocalDamageReceived(ulong clientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (clientId == this._localClientId) { return; }
        RpcSystem.Instance.OnPlayerDamageReceivedServerRpc(clientId, damageType, damageAmount);
    }

    private void OnServerDamageReceived(ulong clientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (!this._playersMap.TryGetValue(clientId, out SoldierController player)) { return; }
        if (clientId != this._localClientId) { return; }

        player.TakeServerDamage(damageType, damageAmount);
        SoldierManager.OnLocalPlayerDamageReceived?.Invoke();
    }

    public void SpawnPlayers()
    {
        if (!this.IsHost || this._hasSpawnedPlayers) { return; }
        this._logger.Log("Spawning soldiers");

        ulong[] connectedClientIds = Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds);

        for (int i = 0; i < connectedClientIds.Count(); i++)
        {
            ulong clientId = connectedClientIds[i];
            this.SpawnPlayer(clientId, i);
        }

        this._logger.Log("Spawned soldiers");
        this._hasSpawnedPlayers = true;
        RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameStarted);
    }

    private void SpawnPlayer(ulong clientId, int spawnPointIndex = -1)
    {
        if (!this.IsHost) { return; }
        if (spawnPointIndex != -1 && spawnPointIndex < 0)
        {
            this._logger.Log($"Cannot spawn player for {clientId} with a spawn index of {spawnPointIndex}", Logger.LogLevel.Error);
            return;
        }
        if (spawnPointIndex != -1 && spawnPointIndex > this._spawnPoints.Count - 1)
        {
            this._logger.Log($"Cannot spawn player for {clientId} with a spawn index of {spawnPointIndex}. There are {this._spawnPoints.Count} spawn points", Logger.LogLevel.Error);
            return;
        }
        if (this._playersMap.ContainsKey(clientId))
        {
            this._logger.Log($"This player is still alive. Cannot spawn them again.", Logger.LogLevel.Error);
            return;
        }

        spawnPointIndex = spawnPointIndex != -1 ? spawnPointIndex : UnityEngine.Random.Range(0, this._spawnPoints.Count);
        Transform spawnPoint = this._spawnPoints[spawnPointIndex];

        Transform playerTransform = Instantiate(this._playerPrefab, spawnPoint.position, Quaternion.identity);
        playerTransform.rotation = spawnPoint.rotation;
        playerTransform.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        this._logger.Log($"Spawned soldier for client ID {clientId}");
    }

    private void OnPlayerRequestSpawn(ulong clientId) => this.SpawnPlayer(clientId);
}
