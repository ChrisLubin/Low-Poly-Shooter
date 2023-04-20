using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RpcSystem : NetworkedStaticInstanceWithLogger<RpcSystem>
{
    public static Action<ulong, ulong[]> OnPlayerGameSceneLoaded;
    public static Action<int> OnPlayerShot;

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
    public void OnPlayerShotServerRpc(ulong shooterClientId, int playerIndex)
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

        this.OnPlayerShotClientRpc(playerIndex, rpcParams);
    }
    [ClientRpc]
    private void OnPlayerShotClientRpc(int playerIndex, ClientRpcParams _ = default) => RpcSystem.OnPlayerShot?.Invoke(playerIndex);
}
