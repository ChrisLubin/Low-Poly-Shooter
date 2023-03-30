using System;

public class GameManager : StaticInstanceWithLogger<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;
    public GameState State { get; private set; }

    private void Start() => ChangeState(GameState.Starting);

    public void ChangeState(GameState newState)
    {
        if (this.State == newState) { return; }

        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState)
        {
            case GameState.Starting:
                this.HandleStarting();
                break;
            case GameState.InGame:
                this.HandleInGame();
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

        OnAfterStateChanged?.Invoke(newState);

        this._logger.Log($"New state: {newState}");
    }

    private void HandleStarting()
    {
        SoldierManager.Instance.SpawnSoldiers();
        ChangeState(GameState.InGame);
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
    None = 0,
    Starting = 1,
    InGame = 2,
    Win = 3,
    Lose = 4,
}
