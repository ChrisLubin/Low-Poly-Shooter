using System;

public class GameManager : NetworkedStaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnStateChange;
    public GameState State { get; private set; }

    public void ChangeState(GameState newState)
    {
        if (this.State == newState) { return; }

        this._logger.Log($"New state: {newState}");
        OnStateChange?.Invoke(newState);

        this.State = newState;
        switch (newState)
        {
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
    GameStarting,
    GameStarted,
    Win,
    Lose,
}
