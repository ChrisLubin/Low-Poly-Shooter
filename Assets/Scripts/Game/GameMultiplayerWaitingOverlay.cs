using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMultiplayerWaitingOverlay : NetworkBehaviourWithLogger<GameMultiplayerWaitingOverlay>
{
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private TextMeshProUGUI _subheaderText;
    [SerializeField] private TextMeshProUGUI _playersListText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _quitButton;

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnStateChange += this.OnGameStateChange;
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

        this._headerText.text = $"Waiting For {(this.IsHost ? "Players" : "Host")} To {(this.IsHost ? "Join" : "Start The Match")}...";
        this.UpdatePlayerList();
        MultiplayerSystem.Instance.PlayerData.OnListChanged += this.OnPlayerDataChanged;

        if (!this.IsHost)
        {
            MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
            return;
        }

        this._startGameButton.gameObject.SetActive(true);
        this._startGameButton.onClick.AddListener(this.OnStartGameButtonClick);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.OnStateChange -= this.OnGameStateChange;
        this._quitButton.onClick.RemoveListener(this.OnQuitButtonClick);

        if (!MultiplayerSystem.IsMultiplayer) { return; }
        MultiplayerSystem.Instance.PlayerData.OnListChanged -= this.OnPlayerDataChanged;

        if (!this.IsHost)
        {
            MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
            return;
        }
        this._startGameButton.onClick.RemoveListener(this.OnStartGameButtonClick);
    }

    private void OnStartGameButtonClick()
    {
        if (!this.IsHost) { return; }
        RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameStarting);
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarting:
                this._headerText.text = "Starting game...";
                this._startGameButton.interactable = false;
                break;
            case GameState.GameStarted:
                this.gameObject.SetActive(false);
                break;
        }
    }

    private void OnPlayerDataChanged(NetworkListEvent<PlayerData> _)
    {
        if (!this.IsHost)
        {
            this._logger.Log($"Player Data list changed. Total players: {MultiplayerSystem.Instance.PlayerData.Count}");
        }
        this.UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        this._playersListText.text = "";
        int playersLoadingCount = 0;

        foreach (PlayerData playerData in MultiplayerSystem.Instance.PlayerData)
        {
            FixedString64Bytes textToAdd = playerData.Username;

            if (playerData.UnityId == AuthenticationService.Instance.PlayerId)
            {
                textToAdd += " (You)";
            }
            else if (playerData.UnityId == MultiplayerSystem.Instance.HostUnityId)
            {
                textToAdd += " (Host)";
            }
            else if (playerData.ClientId == PlayerData.UNREGISTERED_CLIENT_ID)
            {
                textToAdd += " (Loading...)";
                playersLoadingCount++;
            }

            this._playersListText.text += $"{textToAdd}\n\n";
        }

        if (!this.IsHost) { return; }

        this._startGameButton.interactable = MultiplayerSystem.Instance.PlayerData.Count >= 2 && playersLoadingCount == 0;
    }

    private void OnHostDisconnect()
    {
        this._headerText.text = "The host has left the lobby. Please return to the main menu.";
        this._subheaderText.text = "";
        this._playersListText.text = "";
    }

    private void OnQuitButtonClick()
    {
        MultiplayerSystem.QuitMultiplayer();
        SceneManager.LoadScene("MainMenuScene");
    }
}
