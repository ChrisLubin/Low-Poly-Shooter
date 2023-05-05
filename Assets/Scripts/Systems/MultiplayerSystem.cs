using System;
using System.Collections.Generic;
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
    public static event Action OnError;
    public static bool IsMultiplayer { get; private set; } = false;
    public static MultiplayerState State { get; private set; }
    private const string _LOBBY_RELAY_CODE_KEY = "RELAY_CODE";
    private const int _MAX_PLAYER_COUNT = 7;
    public NetworkVariable<ulong> HostId { get; private set; }
    public NetworkList<ulong> ConnectedClientIds { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        RpcSystem.OnMultiplayerStateChange += this.ChangeState;
        this.HostId = new();
        this.ConnectedClientIds = new();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        RpcSystem.OnMultiplayerStateChange -= this.ChangeState;
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= this.OnClientConnected;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!this.IsHost) { return; }

        this.HostId.Value = NetworkManager.Singleton.LocalClientId;
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

        switch (newState)
        {
            case MultiplayerState.NotConnected:
                NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnected;
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
                break;
            case MultiplayerState.CreatingLobby:
                CreateLobbyOptions lobbyOptions = new() { IsPrivate = false };
                string lobbyName = $"{UnityEngine.Random.Range(1, 9999999)}";

                try
                {
                    // Create allocation
                    Allocation createAllocation = await RelayService.Instance.CreateAllocationAsync(_MAX_PLAYER_COUNT);
                    string relayCode = await RelayService.Instance.GetJoinCodeAsync(createAllocation.AllocationId);
                    relayServerData = new(createAllocation, "dtls");
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                    Dictionary<string, DataObject> lobbyData = new() { { _LOBBY_RELAY_CODE_KEY, new DataObject(VisibilityOptions.Member, relayCode) } };
                    lobbyOptions.Data = lobbyData;
                    this._logger.Log($"Lobby Relay code: {relayCode}");

                    await LobbyService.Instance.CreateLobbyAsync(lobbyName, _MAX_PLAYER_COUNT, lobbyOptions);
                    this._logger.Log($"Created lobby {lobbyName}");

                    NetworkManager.Singleton.StartHost();
                    this._logger.Log("Started host");
                    this.ChangeState(MultiplayerState.CreatedLobby);
                }
                catch (Exception e)
                {
                    this._logger.Log(e.Message, Logger.LogLevel.Error);
                    MultiplayerSystem.OnError?.Invoke();
                    this.ChangeState(MultiplayerState.Connected);
                }
                break;
            case MultiplayerState.JoiningLobby:
                try
                {
                    Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                    this._logger.Log($"Joined lobby {lobby.Name}");

                    if (!lobby.Data.TryGetValue(_LOBBY_RELAY_CODE_KEY, out DataObject joinedLobbyData))
                    {
                        this._logger.Log("Unable to get Relay code from lobby", Logger.LogLevel.Error);
                        return;
                    }

                    this._logger.Log($"Lobby Relay code: {joinedLobbyData.Value}");
                    JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinedLobbyData.Value);
                    relayServerData = new(joinAllocation, "dtls");
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                    NetworkManager.Singleton.StartClient();
                    this._logger.Log("Started client");
                    this.ChangeState(MultiplayerState.JoinedLobby);
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

    private void OnClientConnected(ulong clientId)
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
