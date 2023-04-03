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

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform spawnPoint in this._spawnPointsParent)
        {
            this._spawnPoints.Add(spawnPoint);
        }
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

            Transform player = Instantiate(this._playerPrefab, spawnPoint.position, Quaternion.identity, this._spawnedSoldiersParent);
            player.rotation = spawnPoint.rotation;
            player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            this._logger.Log($"Spawned soldier for client ID {clientId}");
        }

        this._logger.Log("Spawned soldiers");
        this._hasSpawnedSoldiers = true;
        RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameStarted);
    }
}
