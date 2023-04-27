using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RpcSystem : NetworkedStaticInstanceWithLogger<RpcSystem>
{
    public static Action<ulong, ulong[]> OnPlayerGameSceneLoaded;
    public static Action<ulong> OnPlayerShot;
    public static Action<ulong, SoldierDamageController.DamageType, int> OnPlayerDamageReceived;

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
    private void ChangeMultiplayerStateClientRpc(MultiplayerState state) => MultiplayerSystem.Instance.ChangeState(state);

    [ServerRpc]
    public void ChangeGameStateServerRpc(GameState state) => this.ChangeGameStateClientRpc(state);
    [ClientRpc]
    private void ChangeGameStateClientRpc(GameState state) => GameManager.Instance.ChangeState(state);

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerShotServerRpc(ulong shooterClientId, ulong shotClientId)
    {
        ulong[] allClientIds = Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds);

        // Send to all clients except the shooter
        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = allClientIds.Where((ulong clientId) => clientId != shooterClientId).ToArray()
            }
        };

        this.OnPlayerShotClientRpc(shotClientId, rpcParams);
    }
    [ClientRpc]
    private void OnPlayerShotClientRpc(ulong clientId, ClientRpcParams _ = default) => RpcSystem.OnPlayerShot?.Invoke(clientId);

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerDamageReceivedServerRpc(ulong originClientId, ulong damagedSoldierClientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        ulong[] allClientIds = Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds);

        // Send to all clients except the origin client
        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = allClientIds.Where((ulong clientId) => clientId != originClientId).ToArray()
            }
        };

        this.OnPlayerDamageReceivedClientRpc(damagedSoldierClientId, damageType, damageAmount, rpcParams);
    }
    [ClientRpc]
    private void OnPlayerDamageReceivedClientRpc(ulong damagedSoldierClientId, SoldierDamageController.DamageType damageType, int damageAmount, ClientRpcParams _ = default) => RpcSystem.OnPlayerDamageReceived?.Invoke(damagedSoldierClientId, damageType, damageAmount);
}
