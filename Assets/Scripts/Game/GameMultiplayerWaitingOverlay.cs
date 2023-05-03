using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameMultiplayerWaitingOverlay : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _waitForText;
    [SerializeField] private TextMeshProUGUI _playersListText;
    [SerializeField] private Button _startGameButton;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        RpcSystem.OnPlayerGameSceneLoaded += this.OnPlayerGameSceneLoaded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!MultiplayerSystem.IsMultiplayer)
        {
            // Doing singleplayer
            this.gameObject.SetActive(false);
            return;
        }

        this._waitForText.text = $"Waiting For {(this.IsHost ? "Players" : "Host")} To {(this.IsHost ? "Join" : "Start The Match")}...";

        if (!this.IsHost)
        {
            return;
        }

        this._startGameButton.gameObject.SetActive(true);
        this._startGameButton.onClick.AddListener(this.StartGame);
    }

    public override void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        RpcSystem.OnPlayerGameSceneLoaded -= this.OnPlayerGameSceneLoaded;
        this._startGameButton.onClick.RemoveListener(this.StartGame);
        base.OnDestroy();
    }

    private void StartGame()
    {
        RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameStarting);
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarting:
                this._waitForText.text = "Starting game...";
                this._startGameButton.interactable = false;
                break;
            case GameState.GameStarted:
                this.gameObject.SetActive(false);
                break;
        }
    }

    private void OnPlayerGameSceneLoaded(ulong hostId, ulong[] connectedClientIds)
    {
        if (this.IsHost && connectedClientIds.Count() >= 2)
        {
            this._startGameButton.interactable = true;
        }
        this.UpdatePlayerList(hostId, connectedClientIds);
    }

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
