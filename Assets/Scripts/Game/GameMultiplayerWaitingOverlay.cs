using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMultiplayerWaitingOverlay : NetworkBehaviourWithLogger<GameMultiplayerWaitingOverlay>
{
    [SerializeField] private TextMeshProUGUI _waitForText;
    [SerializeField] private TextMeshProUGUI _playersListText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _quitButton;

    private NetworkList<PlayerState> _playerStates;

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnStateChange += this.OnGameStateChange;
        this._quitButton.onClick.AddListener(this.OnQuitButtonClick);
        this._playerStates = new();
        this._playerStates.OnListChanged += this.OnPlayerStatesChanged;
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
        this.UpdatePlayerList();

        if (!this.IsHost) { return; }

        RpcSystem.OnPlayerGameSceneLoaded += this.OnPlayerGameSceneLoaded;
        MultiplayerSystem.OnPlayerJoinedLobby += this.OnPlayerJoinedLobby;
        this._startGameButton.gameObject.SetActive(true);
        this._startGameButton.onClick.AddListener(this.StartGame);
        this._playerStates.Add(new PlayerState(AuthenticationService.Instance.PlayerId, MultiplayerSystem.LocalPlayerName, LoadingState.Loaded));
    }

    public override void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        this._startGameButton.onClick.RemoveListener(this.StartGame);
        this._quitButton.onClick.RemoveListener(this.OnQuitButtonClick);

        if (this.IsHost)
        {
            RpcSystem.OnPlayerGameSceneLoaded -= this.OnPlayerGameSceneLoaded;
            MultiplayerSystem.OnPlayerJoinedLobby -= this.OnPlayerJoinedLobby;
        }

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

    private void OnPlayerJoinedLobby(string playerUnityId, string playerName)
    {
        if (!this.IsHost) { return; }

        this._playerStates.Add(new PlayerState(playerUnityId, playerName, LoadingState.Loading));
        this.UpdatePlayerList();
    }

    private void OnPlayerGameSceneLoaded(string playerUnityId, string playerName)
    {
        if (!this.IsHost) { return; }

        bool didFindPlayer = false;

        for (int i = 0; i < this._playerStates.Count; i++)
        {
            PlayerState playerState = this._playerStates[i];
            if (playerState.Id != playerUnityId) { continue; }

            didFindPlayer = true;
            playerState.LoadingState = LoadingState.Loaded;
            this._playerStates[i] = playerState;
            break;
        }

        if (!didFindPlayer)
        {
            this._playerStates.Add(new PlayerState(playerUnityId, playerName, LoadingState.Loaded));
        }

        this.UpdatePlayerList();
    }

    private void OnPlayerStatesChanged(NetworkListEvent<PlayerState> _)
    {
        this._logger.Log("Player state list changed");
        this.UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        this._playersListText.text = "";
        int loadingPlayersCount = 0;

        foreach (PlayerState playerState in this._playerStates)
        {
            FixedString64Bytes textToAdd = playerState.Name;

            if (playerState.Id == AuthenticationService.Instance.PlayerId)
            {
                textToAdd += " (You)";
            }
            else if (playerState.Id == MultiplayerSystem.Instance.HostUnityId.Value)
            {
                textToAdd += " (Host)";
            }
            else if (playerState.LoadingState == LoadingState.Loading)
            {
                textToAdd += " (Loading...)";
                loadingPlayersCount++;
            }

            this._playersListText.text += $"{textToAdd}\n\n";
        }

        if (this.IsHost && this._playerStates.Count >= 2 && loadingPlayersCount == 0)
        {
            this._startGameButton.interactable = true;
        }
    }

    private void OnQuitButtonClick()
    {
        MultiplayerSystem.QuitMultiplayer();
        SceneManager.LoadScene("MainMenuScene");
    }

    [Serializable]
    private struct PlayerState : INetworkSerializable, System.IEquatable<PlayerState>
    {
        public FixedString64Bytes Id;
        public FixedString64Bytes Name;
        public LoadingState LoadingState;

        public PlayerState(FixedString64Bytes id, FixedString64Bytes name, LoadingState loadingState)
        {
            this.Id = id;
            this.Name = name;
            this.LoadingState = loadingState;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Id);
                reader.ReadValueSafe(out Name);
                reader.ReadValueSafe(out LoadingState);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Id);
                writer.WriteValueSafe(Name);
                writer.WriteValueSafe(LoadingState);
            }
        }

        public readonly bool Equals(PlayerState other) => other.Equals(this) && Id == other.Id && Name == other.Name && LoadingState == other.LoadingState;
    }

    private enum LoadingState
    {
        Loading,
        Loaded,
    }
}
