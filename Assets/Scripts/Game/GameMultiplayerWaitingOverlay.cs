using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMultiplayerWaitingOverlay : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _waitForText;
    [SerializeField] private TextMeshProUGUI _playersListText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        RpcSystem.OnPlayerGameSceneLoaded += this.OnPlayerGameSceneLoaded;
        this._quitButton.onClick.AddListener(this.OnQuitButtonClick);
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
        MultiplayerSystem.Instance.ConnectedClientIds.OnListChanged += this.OnConnectedClientIdsChange;

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
        MultiplayerSystem.Instance.ConnectedClientIds.OnListChanged -= this.OnConnectedClientIdsChange;
        this._startGameButton.onClick.RemoveListener(this.StartGame);
        this._quitButton.onClick.RemoveListener(this.OnQuitButtonClick);
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

    private void OnPlayerGameSceneLoaded(ulong joinedClientId) => this.UpdatePlayerList();
    private void OnConnectedClientIdsChange(NetworkListEvent<ulong> _) => this.UpdatePlayerList();

    private void UpdatePlayerList()
    {
        this._playersListText.text = "";

        foreach (ulong connectedClientId in MultiplayerSystem.Instance.ConnectedClientIds)
        {
            string textToAdd = "";
            textToAdd += connectedClientId;

            if (connectedClientId == NetworkManager.Singleton.LocalClientId)
            {
                textToAdd += " (You)";
            }
            else if (connectedClientId == MultiplayerSystem.Instance.HostId.Value)
            {
                textToAdd += " (Host)";
            }

            this._playersListText.text += $"{textToAdd}\n\n";
        }

        if (this.IsHost && MultiplayerSystem.Instance.ConnectedClientIds.Count >= 2)
        {
            this._startGameButton.interactable = true;
        }
    }

    private void OnQuitButtonClick()
    {
        MultiplayerSystem.QuitMultiplayer();
        SceneManager.LoadScene("MainMenuScene");
    }
}
