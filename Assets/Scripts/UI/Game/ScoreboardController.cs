using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI.TableUI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class ScoreboardController : NetworkBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TableUI _table;
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private Button _quitButton;

    private bool _didHostDisconnect = false;
    private bool _isGameNearingEnd = false;

    private static RowData[] _rows = new RowData[0];

    private const int _PLAYER_NAME_COLUMN_INDEX = 0;
    private const int _PLAYER_KILLS_COLUMN_INDEX = 1;
    private const int _PLAYER_DEATHS_COLUMN_INDEX = 2;
    private const int _KILLS_NEEDED_TO_WIN = 25;
    public const float NEARING_END_GAME_PERCENT_THRESHOLD = 0.79f; // 0 - 1 range

    public static event Action OnGameNearingEndReached;

    private void Awake()
    {
        if (!MultiplayerSystem.IsMultiplayer) { return; }

        GameManager.OnStateChange += this.OnGameStateChange;
        MultiplayerSystem.OnPlayerDisconnect += this.OnPlayerDisconnect;
        MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
        this._quitButton.onClick.AddListener(this.OnQuitClick);
    }

    private void Start()
    {
        this._headerText.text = $"First To {_KILLS_NEEDED_TO_WIN} Wins!";

        // Needed so table asset works correctly
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
        MultiplayerSystem.OnPlayerDisconnect -= this.OnPlayerDisconnect;
        MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
        SoldierManager.OnPlayerDeath -= this.OnPlayerDeath;
        this._quitButton.onClick.RemoveListener(this.OnQuitClick);
    }

    private void Update()
    {
        if (!MultiplayerSystem.IsMultiplayer) { return; }

        this._canvas.enabled = GameManager.State == GameState.GameOver || this._didHostDisconnect || Input.GetKey(KeyCode.Tab) && (GameManager.State == GameState.GameStarting || GameManager.State == GameState.GameStarted);
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarting:
                this.InitPlayerRows();
                break;
            case GameState.GameOver:
                this._quitButton.gameObject.SetActive(true);
                ulong winningPlayerClientId = ScoreboardController._rows[0].ClientId;

                if (winningPlayerClientId == NetworkManager.Singleton.LocalClientId)
                    this._headerText.text = $"You Won!";
                else
                    this._headerText.text = $"{MultiplayerSystem.Instance.GetPlayerUsername(ScoreboardController._rows[0].ClientId)} Won!";
                break;
            default:
                break;
        }
    }

    private void OnPlayerDeath(ulong deadClientId, ulong killerClientId)
    {
        for (int i = 0; i < ScoreboardController._rows.Length; i++)
        {
            RowData row = ScoreboardController._rows[i];

            if (row.ClientId == deadClientId)
            {
                row.Deaths++;
                ScoreboardController._rows[i] = row;
            }
            else if (row.ClientId == killerClientId)
            {
                row.Kills++;
                ScoreboardController._rows[i] = row;

                if (!this._isGameNearingEnd && ((float)row.Kills / (float)_KILLS_NEEDED_TO_WIN) >= NEARING_END_GAME_PERCENT_THRESHOLD)
                {
                    this._isGameNearingEnd = true;
                    ScoreboardController.OnGameNearingEndReached?.Invoke();
                }
                if (this.IsHost && row.Kills == _KILLS_NEEDED_TO_WIN)
                    RpcSystem.Instance.ChangeGameStateServerRpc(GameState.GameOver);
            }
        }

        this.UpdateScoreboard();
    }

    private void InitPlayerRows()
    {
        List<RowData> rows = new();

        foreach (PlayerData player in MultiplayerSystem.Instance.PlayerData)
            rows.Add(new(player.Username.ToString(), player.ClientId, 0, 0));

        ScoreboardController._rows = rows.ToArray();

        this.UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        // Sort by kills
        ScoreboardController._rows = ScoreboardController._rows.OrderByDescending<RowData, int>((row) => row.Kills).ToArray();

        for (int i = 0; i < ScoreboardController._rows.Length; i++)
        {
            if (this._table.Rows <= i + 1)
                this._table.Rows++;

            RowData row = ScoreboardController._rows[i];
            string usernameDisplay = row.Username;

            if (NetworkManager.Singleton.LocalClientId == row.ClientId)
                usernameDisplay += " (You)";
            else if (row.DidLeave)
                usernameDisplay += " (Left)";

            this._table.GetCell(i + 1, _PLAYER_NAME_COLUMN_INDEX).text = usernameDisplay;
            this._table.GetCell(i + 1, _PLAYER_KILLS_COLUMN_INDEX).text = row.Kills.ToString();
            this._table.GetCell(i + 1, _PLAYER_DEATHS_COLUMN_INDEX).text = row.Deaths.ToString();
        }
    }

    private void OnQuitClick()
    {
        MultiplayerSystem.QuitMultiplayer();
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnPlayerDisconnect(PlayerData player)
    {
        if (GameManager.State == GameState.GameOver) { return; }

        for (int i = 0; i < ScoreboardController._rows.Length; i++)
        {
            RowData row = ScoreboardController._rows[i];

            if (row.ClientId != player.ClientId || row.DidLeave) { continue; }
            row.DidLeave = true;
            ScoreboardController._rows[i] = row;
        }

        this.UpdateScoreboard();
    }

    private void OnHostDisconnect()
    {
        if (this.IsHost || GameManager.State == GameState.PlayerWaitingForHostToStart || GameManager.State == GameState.GameOver) { return; }

        this._didHostDisconnect = true;
        this._headerText.fontSize = 50;
        this._headerText.text = "The host has left the lobby. Please return to the main menu.";
        this._quitButton.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static bool IsLocalPlayerInFirstPlace()
    {
        if (!MultiplayerSystem.IsMultiplayer || ScoreboardController._rows.Length == 0) { return false; }

        int highestPlayerKills = ScoreboardController._rows[0].Kills;
        return ScoreboardController._rows.Any(row => row.ClientId == NetworkManager.Singleton.LocalClientId && row.Kills == highestPlayerKills);
    }

    // 0 - 1 range
    public static float GetGamePercentDone()
    {
        if (!MultiplayerSystem.IsMultiplayer || ScoreboardController._rows.Length == 0) { return 0f; }

        return (float)ScoreboardController._rows[0].Kills / (float)_KILLS_NEEDED_TO_WIN;
    }

    private struct RowData
    {
        public string Username;
        public ulong ClientId { get; private set; }
        public int Kills;
        public int Deaths;
        public bool DidLeave;

        public RowData(string username, ulong clientId, int kills, int deaths)
        {
            this.Username = username;
            this.ClientId = clientId;
            this.Kills = kills;
            this.Deaths = deaths;
            this.DidLeave = false;
        }
    }
}
