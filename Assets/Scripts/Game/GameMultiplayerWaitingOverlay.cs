using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameMultiplayerWaitingOverlay : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _waitForText;
    [SerializeField] private TextMeshProUGUI _playersListText;

    private void Awake()
    {
        MultiplayerSystem.OnStateChange += this.OnMultiplayerStateChanged;
        RpcSystem.Instance.OnPlayerGameSceneLoaded += this.OnPlayerGameSceneLoaded;
    }

    private void Start()
    {
        RpcSystem.Instance.PlayerGameSceneLoadedServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        this._waitForText.text = $"Waiting For {(this.IsHost ? "Players" : "Host")} To {(this.IsHost ? "Join" : "Start The Match")}...";
    }

    public override void OnDestroy()
    {
        MultiplayerSystem.OnStateChange -= this.OnMultiplayerStateChanged;
        RpcSystem.Instance.OnPlayerGameSceneLoaded -= this.OnPlayerGameSceneLoaded;
        base.OnDestroy();
    }

    private void OnMultiplayerStateChanged(MultiplayerState state)
    {
        switch (state)
        {
            case MultiplayerState.GameStarted:
                this.gameObject.SetActive(false);
                break;
        }
    }

    private void OnPlayerGameSceneLoaded(ulong hostId, ulong[] connectedClientIds) => this.UpdatePlayerList(hostId, connectedClientIds);

    private void UpdatePlayerList(ulong hostId, ulong[] connectedClientIds)
    {
        this._playersListText.text = "";

        foreach (ulong clientId in connectedClientIds)
        {
            string textToAdd = "";
            textToAdd += clientId;

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                textToAdd += " (You)";
            }
            else if (clientId == hostId)
            {
                textToAdd += " (Host)";
            }

            this._playersListText.text += $"{textToAdd}\n\n";
        }
    }
}
