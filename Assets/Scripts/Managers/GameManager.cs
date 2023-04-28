using System;

public class GameManager : NetworkedStaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnStateChange;
    public GameState State { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        RpcSystem.OnGameStateChange += this.ChangeState;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RpcSystem.OnGameStateChange -= this.ChangeState;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (MultiplayerSystem.Instance.State != MultiplayerState.CreatedLobby && MultiplayerSystem.Instance.State != MultiplayerState.JoinedLobby)
        {
            // Doing singleplayer
            this.ChangeState(GameState.GameStarting);
        }
        else
        {
            this.ChangeState(this.IsHost ? GameState.HostWaitingForPlayers : GameState.PlayerWaitingForHostToStart);
        }
    }

    public void ChangeState(GameState newState)
    {
        if (this.State == newState) { return; }

        this._logger.Log($"New state: {newState}");
        this.State = newState;
        OnStateChange?.Invoke(newState);

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

    private void HandleInGame()
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
