using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI.TableUI;
using System.Linq;

public class ScoreboardController : NetworkBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TableUI _table;

    private RowData[] _rows;

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
            this._canvas.enabled = false;
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

    private void Update()
    {
        if (!MultiplayerSystem.IsMultiplayer) { return; }

        this._canvas.enabled = Input.GetKey(KeyCode.Tab) && (GameManager.State == GameState.GameStarting || GameManager.State == GameState.GameStarted);
    }

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
        for (int i = 0; i < this._rows.Length; i++)
        {
            RowData row = this._rows[i];

            if (row.ClientId == deadClientId)
            {
                row.Deaths++;
                this._rows[i] = row;
            }
            else if (row.ClientId == killerClientId)
            {
                row.Kills++;
                this._rows[i] = row;
            }
        }

        this.UpdateScoreboard();
    }

    private void InitPlayerRows()
    {
        List<RowData> rows = new();

        foreach (PlayerData player in MultiplayerSystem.Instance.PlayerData)
        {
            string usernameDisplay = player.Username.ToString();

            if (NetworkManager.Singleton.LocalClientId == player.ClientId)
                usernameDisplay += " (You)";

            rows.Add(new(usernameDisplay, player.ClientId, 0, 0));
        }

        this._rows = rows.ToArray();

        this.UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        // Sort by kills
        this._rows = this._rows.OrderByDescending<RowData, int>((row) => row.Kills).ToArray();

        for (int i = 0; i < this._rows.Length; i++)
        {
            if (this._table.Rows <= i + 1)
                this._table.Rows++;

            RowData row = this._rows[i];

            this._table.GetCell(i + 1, _PLAYER_NAME_COLUMN_INDEX).text = row.UsernameDisplay;
            this._table.GetCell(i + 1, _PLAYER_KILLS_COLUMN_INDEX).text = row.Kills.ToString();
            this._table.GetCell(i + 1, _PLAYER_DEATHS_COLUMN_INDEX).text = row.Deaths.ToString();
        }
    }

    private struct RowData
    {
        public string UsernameDisplay { get; private set; }
        public ulong ClientId { get; private set; }
        public int Kills;
        public int Deaths;

        public RowData(string usernameDisplay, ulong clientId, int kills, int deaths)
        {
            this.UsernameDisplay = usernameDisplay;
            this.ClientId = clientId;
            this.Kills = kills;
            this.Deaths = deaths;
        }
    }
}
