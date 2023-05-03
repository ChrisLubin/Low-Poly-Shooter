using System;
using System.Linq;
using Unity.Netcode;

public class RpcSystem : NetworkedStaticInstanceWithLogger<RpcSystem>
{
    public static Action<ulong, ulong[]> OnPlayerGameSceneLoaded;
    public static Action<ulong> OnPlayerShoot;
    public static Action<ulong, SoldierDamageController.DamageType, int> OnPlayerDamageReceived;
    public static Action<MultiplayerState> OnMultiplayerStateChange;
    public static Action<GameState> OnGameStateChange;
    public static Action<ulong> OnPlayerRequestSpawn;

    [ServerRpc(RequireOwnership = false)]
    public void PlayerGameSceneLoadedServerRpc(ulong joinedClientId) => this.PlayerGameSceneLoadedClientRpc(this.OwnerClientId, joinedClientId, Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds));
    [ClientRpc]
    private void PlayerGameSceneLoadedClientRpc(ulong hostId, ulong joinedClientId, ulong[] connectedClientIds)
    {
        this._logger.Log($"Client with id {joinedClientId} has loaded their game scene");
        RpcSystem.OnPlayerGameSceneLoaded?.Invoke(hostId, connectedClientIds);
    }

    [ServerRpc]
    public void ChangeMultiplayerStateServerRpc(MultiplayerState state) => this.ChangeMultiplayerStateClientRpc(state);
    [ClientRpc]
    private void ChangeMultiplayerStateClientRpc(MultiplayerState state) => RpcSystem.OnMultiplayerStateChange?.Invoke(state);

    [ServerRpc]
    public void ChangeGameStateServerRpc(GameState state) => this.ChangeGameStateClientRpc(state);
    [ClientRpc]
    private void ChangeGameStateClientRpc(GameState state) => RpcSystem.OnGameStateChange?.Invoke(state);

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerShootServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong[] allClientIds = Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds);

        // Send to all clients except the shooter
        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = allClientIds.Where((ulong clientId) => clientId != serverRpcParams.Receive.SenderClientId).ToArray()
            }
        };

        this.OnPlayerShootClientRpc(serverRpcParams.Receive.SenderClientId, rpcParams);
    }
    [ClientRpc]
    private void OnPlayerShootClientRpc(ulong clientId, ClientRpcParams _ = default) => RpcSystem.OnPlayerShoot?.Invoke(clientId);

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerDamageReceivedServerRpc(ulong damagedSoldierClientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        // Only send to client that was damaged
        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { damagedSoldierClientId }
            }
        };

        this.OnPlayerDamageReceivedClientRpc(damagedSoldierClientId, damageType, damageAmount, rpcParams);
    }
    [ClientRpc]
    private void OnPlayerDamageReceivedClientRpc(ulong damagedSoldierClientId, SoldierDamageController.DamageType damageType, int damageAmount, ClientRpcParams _ = default) => RpcSystem.OnPlayerDamageReceived?.Invoke(damagedSoldierClientId, damageType, damageAmount);

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerSpawnServerRpc(ServerRpcParams serverRpcParams = default) => RpcSystem.OnPlayerRequestSpawn?.Invoke(serverRpcParams.Receive.SenderClientId);
}
