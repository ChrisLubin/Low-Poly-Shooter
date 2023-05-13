using System;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkedStaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnStateChange;
    public static GameState State { get; private set; }
    private GameObject _mainCamera;

    protected override void Awake()
    {
        base.Awake();
        RpcSystem.OnGameStateChange += this.ChangeState;
        SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
        this._mainCamera = Camera.main.gameObject;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RpcSystem.OnGameStateChange -= this.ChangeState;
        SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
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

    private void OnLocalPlayerSpawn() => this._mainCamera.SetActive(false);
    private void OnLocalPlayerDeath() => this._mainCamera.SetActive(true);

    public void ChangeState(GameState newState)
    {
        if (GameManager.State == newState) { return; }

        this._logger.Log($"New state: {newState}");
        GameManager.State = newState;
        GameManager.OnStateChange?.Invoke(newState);

        switch (newState)
        {
            case GameState.HostWaitingForPlayers:
                break;
            case GameState.PlayerWaitingForHostToStart:
                break;
            case GameState.GameStarting:
                this.HandleGameStarting();
                break;
            case GameState.GameStarted:
                this.HandleGameStarted();
                break;
            case GameState.Win:
                this.HandleWin();
                break;
            case GameState.Lose:
                this.HandleLose();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void HandleGameStarting()
    {
    }

    private void HandleGameStarted()
    {
    }

    private void HandleWin()
    {
    }

    private void HandleLose()
    {
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
    Win,
    Lose,
}
