using System;
using DistantLands.Cozy;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkedStaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnStateChange;
    public static GameState State { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _winningSong;
    [SerializeField] private AudioClip _losingSong;

    private const float _WINNING_SONG_MIN_VOLUME = 0.015f;
    private const float _WINNING_SONG_MAX_VOLUME = 0.09f;
    private const float _LOSING_SONG_MIN_VOLUME = 0.06f;
    private const float _LOSING_SONG_MAX_VOLUME = 0.25f;

    protected override void Awake()
    {
        base.Awake();
        RpcSystem.OnGameStateChange += this.ChangeState;
        ScoreboardController.OnGameNearingEndReached += this.OnGameNearingEndReached;
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RpcSystem.OnGameStateChange -= this.ChangeState;
        ScoreboardController.OnGameNearingEndReached -= this.OnGameNearingEndReached;
        SoldierManager.OnPlayerDeath -= this.OnPlayerDeath;
        this.ChangeState(GameState.None);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!MultiplayerSystem.IsMultiplayer)
        {
            this.ChangeState(GameState.GameStarting);
            return;
        }

        RpcSystem.Instance.PlayerGameSceneLoadedServerRpc(AuthenticationService.Instance.PlayerId, MultiplayerSystem.LocalPlayerName);
        this.ChangeState(this.IsHost ? GameState.HostWaitingForPlayers : GameState.PlayerWaitingForHostToStart);
    }

    private void Update()
    {
        if (!this._audioSource.isPlaying) { return; }

        // Make end game song louder as game gets closer to ending
        if (this._audioSource.clip == this._winningSong)
            this._audioSource.volume = Mathf.Lerp(_WINNING_SONG_MIN_VOLUME, _WINNING_SONG_MAX_VOLUME, Mathf.InverseLerp(ScoreboardController.NEARING_END_GAME_PERCENT_THRESHOLD, 1f, ScoreboardController.GetGamePercentDone()));
        else if (this._audioSource.clip == this._losingSong)
            this._audioSource.volume = Mathf.Lerp(_LOSING_SONG_MIN_VOLUME, _LOSING_SONG_MAX_VOLUME, Mathf.InverseLerp(ScoreboardController.NEARING_END_GAME_PERCENT_THRESHOLD, 1f, ScoreboardController.GetGamePercentDone()));
    }

    public void ChangeState(GameState newState)
    {
        if (GameManager.State == newState) { return; }

        this._logger.Log($"New state: {newState}");
        GameManager.State = newState;
        GameManager.OnStateChange?.Invoke(newState);

        switch (newState)
        {
            case GameState.None:
                break;
            case GameState.HostWaitingForPlayers:
                HandleHostWaitingForPlayers();
                break;
            case GameState.PlayerWaitingForHostToStart:
                HandlePlayerWaitingForHostToStart();
                break;
            case GameState.GameStarting:
                this.HandleGameStarting();
                break;
            case GameState.GameStarted:
                this.HandleGameStarted();
                break;
            case GameState.GameOver:
                this.HandleGameOver();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void HandleHostWaitingForPlayers()
    {
        CozyWeather.instance.perennialProfile.pauseTime = true;
    }

    private void HandlePlayerWaitingForHostToStart()
    {
        CozyWeather.instance.perennialProfile.pauseTime = true;
    }

    private void HandleGameStarting()
    {
    }

    private void HandleGameStarted()
    {
        CozyWeather.instance.perennialProfile.pauseTime = false;
    }

    private void HandleGameOver()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGameNearingEndReached()
    {
        this._audioSource.clip = ScoreboardController.IsLocalPlayerInFirstPlace() ? this._winningSong : this._losingSong;
        this._audioSource.Play();
        this._logger.Log($"The game is nearing end so playing {(ScoreboardController.IsLocalPlayerInFirstPlace() ? "winning" : "losing")} song!");
    }

    private void OnPlayerDeath(ulong _, ulong __)
    {
        if (!this._audioSource.isPlaying) { return; }

        if (this._audioSource.clip == this._losingSong && ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._logger.Log("Local player gained the lead in end game.");
            this._audioSource.clip = this._winningSong;
            this._audioSource.Play();
        }
        else if (this._audioSource.clip == this._winningSong && !ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._logger.Log("Local player lost lead in end game.");
            this._audioSource.clip = this._losingSong;
            this._audioSource.Play();
        }
    }
}

[Serializable]
public enum GameState
{
    None,
    HostWaitingForPlayers,
    PlayerWaitingForHostToStart,
    GameStarting,
    GameStarted,
    GameOver
}
