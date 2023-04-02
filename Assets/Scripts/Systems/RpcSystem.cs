using System;
using Unity.Netcode;

public class RpcSystem : NetworkedStaticInstanceWithLogger<RpcSystem>
{
    public static Action<ulong, ulong[]> OnPlayerGameSceneLoaded;

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
}
