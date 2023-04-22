using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SoldierManager : NetworkedStaticInstanceWithLogger<SoldierManager>
{
    [SerializeField] private Transform _spawnedSoldiersParent;
    [SerializeField] private Transform _spawnPointsParent;
    private List<Transform> _spawnPoints = new();
    private bool _hasSpawnedSoldiers = false;
    [SerializeField] Transform _playerPrefab;
    private List<SoldierController> _players = new();
    private SoldierController _localPlayer;

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
        SoldierController.OnDespawn += this.OnDespawn;
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
        SoldierController.OnDespawn -= this.OnDespawn;
        SoldierController.OnShot -= this.OnShot;
        SoldierController.OnDamageReceived -= this.OnLocalDamageReceived;
        RpcSystem.OnPlayerShot -= this.OnServerShot;
        RpcSystem.OnPlayerDamageReceived -= this.OnServerDamageReceived;
    }

    private void OnLocalPlayerSpawn(SoldierController player) => this._localPlayer = player;
    private void OnSpawn(SoldierController player) => this._players.Add(player);
    private void OnDespawn(SoldierController player) => this._players.Remove(player);
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

    private void OnLocalDamageReceived(SoldierController player, SoldierDamageController.DamageType damageType)
    {
        if (player == this._localPlayer) { return; }
        RpcSystem.Instance.OnPlayerDamageReceivedServerRpc(NetworkManager.Singleton.LocalClientId, this._players.IndexOf(player), damageType);
    }

    private void OnServerDamageReceived(int playerIndex, SoldierDamageController.DamageType damageType)
    {
        SoldierController player = this._players.ElementAtOrDefault(playerIndex);

        if (!player)
        {
            this._logger.Log("Unable to find player who took damage", Logger.LogLevel.Error);
            return;
        }
        if (player != this._localPlayer) { return; }

        player.TakeServerDamage(damageType);
        SoldierManager.OnLocalPlayerDamageReceived?.Invoke();
    }

    public void SpawnSoldiers()
    {
        if (!this.IsHost || this._hasSpawnedSoldiers) { return; }
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
        this._hasSpawnedSoldiers = true;
        RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameStarted);
    }
}
