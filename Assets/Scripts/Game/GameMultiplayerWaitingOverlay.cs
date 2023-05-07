using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMultiplayerWaitingOverlay : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _waitForText;
    [SerializeField] private TextMeshProUGUI _playersListText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _quitButton;

    private List<string> _playerUnityIdsCurrentlyLoading = new();

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        RpcSystem.OnPlayerGameSceneLoaded += this.OnPlayerGameSceneLoaded;
        MultiplayerSystem.OnPlayerJoinedLobby += this.OnPlayerJoinedLobby;
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
        MultiplayerSystem.OnPlayerJoinedLobby -= this.OnPlayerJoinedLobby;
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

    private void OnPlayerJoinedLobby(string playerUnityId, string _)
    {
        this._playerUnityIdsCurrentlyLoading.Add(playerUnityId);
        this.UpdatePlayerList();
    }

    private void OnPlayerGameSceneLoaded(string playerUnityId, string _)
    {
        this._playerUnityIdsCurrentlyLoading.Remove(playerUnityId);
        this.UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        this._playersListText.text = "";
        Dictionary<string, string> playerIdToNameMap = MultiplayerSystem.Instance.GetPlayersInLobbyWithName();

        foreach (KeyValuePair<string, string> playerIdAndMapPair in playerIdToNameMap)
        {
            string playerUnityId = playerIdAndMapPair.Key;
            string playerName = playerIdAndMapPair.Value;
            string textToAdd = playerName;

            if (playerUnityId == AuthenticationService.Instance.PlayerId)
            {
                textToAdd += " (You)";
            }
            else if (playerUnityId == MultiplayerSystem.Instance.HostUnityId.Value)
            {
                textToAdd += " (Host)";
            }
            else if (this._playerUnityIdsCurrentlyLoading.Contains(playerUnityId))
            {
                textToAdd += " (Loading...)";
            }

            this._playersListText.text += $"{textToAdd}\n\n";
        }

        if (this.IsHost && playerIdToNameMap.Count >= 2 && this._playerUnityIdsCurrentlyLoading.Count == 0)
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
