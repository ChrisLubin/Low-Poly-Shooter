using System;

public class GameManager : NetworkedStaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnStateChange;
    public GameState State { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        this.ChangeState(this.IsHost ? GameState.HostWaitingForPlayers : GameState.PlayerWaitingForHostToStart);
    }

    public void ChangeState(GameState newState)
    {
        if (this.State == newState) { return; }

        this._logger.Log($"New state: {newState}");
        OnStateChange?.Invoke(newState);

        this.State = newState;
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
        if (!this.IsHost) { return; }

        SoldierManager.Instance.SpawnSoldiers();
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
