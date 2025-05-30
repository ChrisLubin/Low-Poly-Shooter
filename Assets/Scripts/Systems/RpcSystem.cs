using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RpcSystem : NetworkedStaticInstanceWithLogger<RpcSystem>
{
    public static event Action<string, string, ulong> OnPlayerGameSceneLoaded;
    public static event Action<ulong> OnPlayerShoot;
    public static event Action<ulong, ulong, Vector3, DamageType, int> OnPlayerTakeDamage;
    public static event Action<MultiplayerState> OnMultiplayerStateChange;
    public static event Action<GameState> OnGameStateChange;
    public static event Action<ulong> OnPlayerRequestSpawn;

    [ServerRpc(RequireOwnership = false)]
    public void PlayerGameSceneLoadedServerRpc(string playerUnityId, string playerUsername, ServerRpcParams serverRpcParams = default)
    {
        this._logger.Log($"{playerUsername} has loaded their game scene");
        RpcSystem.OnPlayerGameSceneLoaded?.Invoke(playerUnityId, playerUsername, serverRpcParams.Receive.SenderClientId);
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
    public void OnPlayerShootServerRpc(ServerRpcParams serverRpcParams = default) => this.OnPlayerShootClientRpc(serverRpcParams.Receive.SenderClientId, serverRpcParams.GetClientRpcParamsWithoutSender());
    [ClientRpc]
    private void OnPlayerShootClientRpc(ulong clientId, ClientRpcParams _ = default) => RpcSystem.OnPlayerShoot?.Invoke(clientId);

    [ServerRpc(RequireOwnership = false)]
    public void OnPlayerTakeDamageServerRpc(ulong damagedSoldierClientId, Vector3 damagePoint, DamageType damageType, int damageAmount, ServerRpcParams serverRpcParams = default) => RpcSystem.OnPlayerTakeDamage?.Invoke(damagedSoldierClientId, serverRpcParams.Receive.SenderClientId, damagePoint, damageType, damageAmount);

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerSpawnServerRpc(ServerRpcParams serverRpcParams = default) => RpcSystem.OnPlayerRequestSpawn?.Invoke(serverRpcParams.Receive.SenderClientId);
}
