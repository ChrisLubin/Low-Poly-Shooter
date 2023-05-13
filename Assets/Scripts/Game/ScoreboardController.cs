using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI.TableUI;

public class ScoreboardController : NetworkBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TableUI _table;

    private const int _PLAYER_NAME_COLUMN_INDEX = 0;
    private const int _PLAYER_KILLS_COLUMN_INDEX = 1;
    private const int _PLAYER_DEATHS_COLUMN_INDEX = 2;

    private void Awake()
    {
        if (!MultiplayerSystem.IsMultiplayer) { return; }

        GameManager.OnStateChange += this.OnGameStateChange;
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
    }

    private void Start()
    {
        // Needed so table package works correctly
        this._canvas.enabled = true;
        this._canvas.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!MultiplayerSystem.IsMultiplayer)
        {
            this.gameObject.SetActive(false);
            this._canvas.enabled = false;
            this.enabled = false;
            return;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!MultiplayerSystem.IsMultiplayer) { return; }

        GameManager.OnStateChange -= this.OnGameStateChange;
        SoldierManager.OnPlayerDeath -= this.OnPlayerDeath;
    }

    private void Update() => this._canvas.enabled = Input.GetKey(KeyCode.Tab) && (GameManager.State == GameState.GameStarting || GameManager.State == GameState.GameStarted);

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarting:
                this.InitPlayerRows();
                break;
            default:
                break;
        }
    }

    private void OnPlayerDeath(ulong deadClientId, ulong killerClientId)
    {
        string killerPlayerName = MultiplayerSystem.Instance.GetPlayerUsername(killerClientId);
        string deadPlayerName = MultiplayerSystem.Instance.GetPlayerUsername(deadClientId);

        for (int i = 1; i < this._table.Rows; i++)
        {
            string rowPlayerName = this._table.GetCell(i, _PLAYER_NAME_COLUMN_INDEX).text;
            if (rowPlayerName != deadPlayerName && rowPlayerName != killerPlayerName) { continue; }
            string rowPlayerKills = this._table.GetCell(i, _PLAYER_KILLS_COLUMN_INDEX).text;
            string rowPlayerDeaths = this._table.GetCell(i, _PLAYER_DEATHS_COLUMN_INDEX).text;

            this._table.GetCell(i, _PLAYER_KILLS_COLUMN_INDEX).text = rowPlayerName == killerPlayerName ? (int.Parse(rowPlayerKills) + 1).ToString() : rowPlayerKills;
            this._table.GetCell(i, _PLAYER_DEATHS_COLUMN_INDEX).text = rowPlayerName == deadPlayerName ? (int.Parse(rowPlayerDeaths) + 1).ToString() : rowPlayerDeaths;
        }
    }

    private void InitPlayerRows()
    {
        // Init player rows
        foreach (PlayerData player in MultiplayerSystem.Instance.PlayerData)
        {
            this._table.Rows++;
            int lastRowIndex = this._table.Rows - 1;

            this._table.GetCell(lastRowIndex, _PLAYER_NAME_COLUMN_INDEX).text = player.Username.ToString();
            this._table.GetCell(lastRowIndex, _PLAYER_KILLS_COLUMN_INDEX).text = "0";
            this._table.GetCell(lastRowIndex, _PLAYER_DEATHS_COLUMN_INDEX).text = "0";
        }
    }
}
