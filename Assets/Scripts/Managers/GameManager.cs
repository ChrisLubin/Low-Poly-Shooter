using System;
using DistantLands.Cozy;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkedStaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnStateChange;
    public static GameState State { get; private set; }

    [SerializeField] private AudioSource _soundEffectAudioSource;
    [SerializeField] private AudioClip _missionFailedAudioClip;

    protected override void Awake()
    {
        base.Awake();
        RpcSystem.OnGameStateChange += this.ChangeState;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RpcSystem.OnGameStateChange -= this.ChangeState;
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

        if (MultiplayerSystem.IsMultiplayer && !ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._soundEffectAudioSource.clip = this._missionFailedAudioClip;
            this._soundEffectAudioSource.Play();
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
