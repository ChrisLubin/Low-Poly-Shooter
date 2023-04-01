using System;
using Unity.Netcode;

public class RpcSystem : NetworkedStaticInstanceWithLogger<RpcSystem>
{
    public Action<ulong, ulong[]> OnPlayerGameSceneLoaded;

    [ServerRpc(RequireOwnership = false)]
    public void PlayerGameSceneLoadedServerRpc(ulong joinedClientId) => this.PlayerGameSceneLoadedClientRpc(this.OwnerClientId, joinedClientId, Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds));
    [ClientRpc]
    private void PlayerGameSceneLoadedClientRpc(ulong hostId, ulong joinedClientId, ulong[] connectedClientIds)
    {
        this._logger.Log($"Client with id {joinedClientId} has loaded their game scene");
        this.OnPlayerGameSceneLoaded?.Invoke(hostId, connectedClientIds);
    }
}
