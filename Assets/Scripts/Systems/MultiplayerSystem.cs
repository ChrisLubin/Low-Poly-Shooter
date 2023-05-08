using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using static Unity.Services.Lobbies.Models.DataObject;

public class MultiplayerSystem : NetworkedStaticInstanceWithLogger<MultiplayerSystem>
{
    public static event Action<MultiplayerState> OnStateChange;
    public static event Action<LobbyExceptionReason> OnLobbyError;
    public static event Action OnError;
    public static bool IsMultiplayer { get; private set; } = false;
    public NetworkVariable<FixedString64Bytes> HostUnityId { get; private set; }
    public NetworkList<ulong> ConnectedClientIds { get; private set; } // Clients who are in the networked scene and fully loaded
    public static MultiplayerState State { get; private set; }
    private const int _MAX_PLAYER_COUNT = 7;
    public static string LocalPlayerName { get; private set; }

    public static event Action<string, string> OnPlayerJoinedLobby;
    private const string _LOBBY_RELAY_CODE_KEY = "RELAY_CODE";
    private const string _LOBBY_PLAYER_NAME_KEY = "PLAYER_NAME";
    private LobbyEventCallbacks lobbyEventCallbacks;
    private Lobby _lobby;

    protected override void Awake()
    {
        base.Awake();
        RpcSystem.OnMultiplayerStateChange += this.ChangeState;
        this.HostUnityId = new();
        this.ConnectedClientIds = new();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RpcSystem.OnMultiplayerStateChange -= this.ChangeState;
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= this.OnClientRelayConnected;
        }
        this.DisposeLobby();
    }

    private void DisposeLobby()
    {
        if (this.lobbyEventCallbacks != null)
        {
            this.lobbyEventCallbacks.PlayerJoined -= this._OnPlayerJoinedLobby;
        }
        this.lobbyEventCallbacks = null;
        this._lobby = null;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!this.IsHost) { return; }

        this.HostUnityId.Value = AuthenticationService.Instance.PlayerId;
    }

    private void Start() => this.ChangeState(MultiplayerState.NotConnected);
    private void OnRelaySignIn() => this._logger.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");

    public static void QuitMultiplayer()
    {
        NetworkManager.Singleton.Shutdown();
        MultiplayerSystem.Instance.ChangeState(MultiplayerState.Connected);
    }

    // Referenced in main menu buttons
    public void ChangeState(string newStateString)
    {
        if (!Enum.IsDefined(typeof(MultiplayerState), newStateString))
        {
            this._logger.Log("Invalid state given", Logger.LogLevel.Error);
            return;
        }

        MultiplayerState newStateEnum = Enum.Parse<MultiplayerState>(newStateString);
        this.ChangeState(newStateEnum);
    }

    public async void ChangeState(MultiplayerState newState)
    {
        if (MultiplayerSystem.State == newState) { return; }
        this._logger.Log($"New state: {newState}");
        MultiplayerSystem.State = newState;
        MultiplayerSystem.OnStateChange?.Invoke(newState);

        RelayServerData relayServerData;
        string playerName;

        switch (newState)
        {
            case MultiplayerState.NotConnected:
                NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientRelayConnected;
                string profileName = $"Player-{UnityEngine.Random.Range(1, 9999999)}";
                InitializationOptions initOptions = new();
                initOptions.SetProfile(profileName);
                this._logger.Log($"Set Unity Services profile name to {profileName}");

                await UnityServices.InitializeAsync(initOptions);
                AuthenticationService.Instance.SignedIn += this.OnRelaySignIn;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                this.ChangeState(MultiplayerState.Connected);
                break;
            case MultiplayerState.Connected:
                AuthenticationService.Instance.SignedIn -= this.OnRelaySignIn;
                MultiplayerSystem.IsMultiplayer = false;
                this.DisposeLobby();
                break;
            case MultiplayerState.CreatingLobby:
                string lobbyName = $"{UnityEngine.Random.Range(1, 9999999)}";
                playerName = $"Player-{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}";
                MultiplayerSystem.LocalPlayerName = playerName;

                try
                {
                    // Create allocation
                    Allocation createAllocation = await RelayService.Instance.CreateAllocationAsync(_MAX_PLAYER_COUNT);
                    string relayCode = await RelayService.Instance.GetJoinCodeAsync(createAllocation.AllocationId);
                    relayServerData = new(createAllocation, "dtls");
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                    this._logger.Log($"Lobby Relay code: {relayCode}");

                    NetworkManager.Singleton.StartHost();
                    this._logger.Log("Started host");

                    CreateLobbyOptions createLobbyOptions = new()
                    {
                        IsPrivate = true,
                        Player = new(null, null, null, createAllocation.AllocationId.ToString())
                        {
                            Data = new() { { _LOBBY_PLAYER_NAME_KEY, new(PlayerDataObject.VisibilityOptions.Member, playerName) } },
                        },
                        Data = new() { { _LOBBY_RELAY_CODE_KEY, new DataObject(VisibilityOptions.Member, relayCode) } }
                    };
                    this._lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, _MAX_PLAYER_COUNT, createLobbyOptions);
                    this._logger.Log($"Created private lobby {lobbyName} as {playerName}");

                    this.lobbyEventCallbacks = new LobbyEventCallbacks();
                    this.lobbyEventCallbacks.PlayerJoined += this._OnPlayerJoinedLobby;
                    await LobbyService.Instance.SubscribeToLobbyEventsAsync(this._lobby.Id, lobbyEventCallbacks);
                    this._logger.Log($"Subscribed to lobby events");
                    this.ChangeState(MultiplayerState.CreatedLobby);
                }
                catch (LobbyServiceException e)
                {
                    this._logger.Log(e.Message, Logger.LogLevel.Error);
                    MultiplayerSystem.OnLobbyError?.Invoke(e.Reason);
                    this.ChangeState(MultiplayerState.Connected);
                }
                catch (Exception e)
                {
                    this._logger.Log(e.Message, Logger.LogLevel.Error);
                    MultiplayerSystem.OnError?.Invoke();
                    this.ChangeState(MultiplayerState.Connected);
                }
                break;
            case MultiplayerState.JoiningLobby:
                playerName = $"Player-{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}";
                MultiplayerSystem.LocalPlayerName = playerName;

                try
                {
                    QuickJoinLobbyOptions joinLobbyOptions = new()
                    {
                        Player = new() { Data = new() { { _LOBBY_PLAYER_NAME_KEY, new(PlayerDataObject.VisibilityOptions.Member, playerName) } } },
                    };
                    this._lobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinLobbyOptions);
                    this._logger.Log($"Joined lobby {this._lobby.Name} as {playerName}");

                    if (!this._lobby.Data.TryGetValue(_LOBBY_RELAY_CODE_KEY, out DataObject joinedLobbyRelayCodeData))
                    {
                        this._logger.Log("Unable to get Relay code from lobby", Logger.LogLevel.Error);
                        return;
                    }

                    this._logger.Log($"Lobby Relay code: {joinedLobbyRelayCodeData.Value}");
                    JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinedLobbyRelayCodeData.Value);
                    relayServerData = new(joinAllocation, "dtls");
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                    NetworkManager.Singleton.StartClient();
                    this._logger.Log("Started client");

                    UpdatePlayerOptions updatePlayerOptions = new() { AllocationId = joinAllocation.AllocationId.ToString() };
                    await LobbyService.Instance.UpdatePlayerAsync(this._lobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
                    this._logger.Log($"Linked Relay allocation ID to player");

                    this.ChangeState(MultiplayerState.JoinedLobby);
                }
                catch (LobbyServiceException e)
                {
                    this._logger.Log(e.Message, Logger.LogLevel.Error);
                    MultiplayerSystem.OnLobbyError?.Invoke(e.Reason);
                    this.ChangeState(MultiplayerState.Connected);
                }
                catch (Exception e)
                {
                    this._logger.Log(e.Message, Logger.LogLevel.Error);
                    MultiplayerSystem.OnError?.Invoke();
                    this.ChangeState(MultiplayerState.Connected);
                }
                break;
            case MultiplayerState.CreatedLobby:
                MultiplayerSystem.IsMultiplayer = true;
                break;
            case MultiplayerState.JoinedLobby:
                MultiplayerSystem.IsMultiplayer = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    public async Task SetLobbyToPublic()
    {
        if (!this.IsHost || this._lobby == null) { return; }

        UpdateLobbyOptions updateLobbyOptions = new() { IsPrivate = false };
        await LobbyService.Instance.UpdateLobbyAsync(this._lobby.Id, updateLobbyOptions);
        this._logger.Log($"Set lobby to public");
    }

    private void _OnPlayerJoinedLobby(List<LobbyPlayerJoined> joinedPlayers)
    {
        if (!this.IsHost) { return; }

        foreach (LobbyPlayerJoined joinedPlayer in joinedPlayers)
        {
            string playerName;
            if (!joinedPlayer.Player.Data.TryGetValue(_LOBBY_PLAYER_NAME_KEY, out PlayerDataObject playerNameData))
            {
                this._logger.Log($"Unable to get player name from player data with ID {joinedPlayer.Player.Id}", Logger.LogLevel.Error);
                playerName = "STRANGE";
            }
            else
            {
                this._logger.Log($"{playerNameData.Value} joined!");
                playerName = playerNameData.Value;
            }

            MultiplayerSystem.OnPlayerJoinedLobby?.Invoke(joinedPlayer.Player.Id, playerName);
        }
    }

    private void OnClientRelayConnected(ulong clientId)
    {
        if (this.IsHost)
        {
            this.ConnectedClientIds.Add(clientId);
        }

        if (this.IsHost && this.OwnerClientId == clientId)
        {
            // Ignore when host first connects
            return;
        }

        if (this.IsHost)
        {
            this._logger.Log($"Client ID {clientId} connected to you.");
        }
        else
        {
            this._logger.Log($"You joined with a client ID of {clientId}.");
        }
    }
}

public enum MultiplayerState
{
    None,
    NotConnected,
    Connected,
    CreatingLobby,
    CreatedLobby,
    JoiningLobby,
    JoinedLobby,
}
