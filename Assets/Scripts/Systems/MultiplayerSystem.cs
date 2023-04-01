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
    public MultiplayerState State { get; private set; }
    private const string LOBBY_RELAY_CODE_KEY = "RELAY_CODE";

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= this.OnClientConnected;
        }
    }

    private void Start() => this.ChangeState(MultiplayerState.NotConnected);

    private void OnRelaySignIn() => this._logger.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");

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
        if (this.State == newState) { return; }
        this._logger.Log($"New state: {newState}");
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
                break;
            case MultiplayerState.CreatingLobby:
                int maxPlayersCount = 7;
                CreateLobbyOptions lobbyOptions = new() { IsPrivate = false };
                string lobbyName = $"{UnityEngine.Random.Range(1, 9999999)}";

                try
                {
                    // Create allocation
                    Allocation createAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayersCount);
                    string relayCode = await RelayService.Instance.GetJoinCodeAsync(createAllocation.AllocationId);
                    relayServerData = new(createAllocation, "dtls");
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                    Dictionary<string, DataObject> lobbyData = new() { { LOBBY_RELAY_CODE_KEY, new DataObject(VisibilityOptions.Member, relayCode) } };
                    lobbyOptions.Data = lobbyData;
                    this._logger.Log($"Lobby Relay code: {relayCode}");

                    await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayersCount, lobbyOptions);
                    this._logger.Log($"Created lobby {lobbyName}");

                    NetworkManager.Singleton.StartHost();
                    this._logger.Log("Started host");
                    this.ChangeState(MultiplayerState.HostWaitingForPlayers);
                }
                catch (RelayServiceException e) { this._logger.Log(e.ToString(), Logger.LogLevel.Error); }
                break;
            case MultiplayerState.HostWaitingForPlayers:
                break;
            case MultiplayerState.JoiningLobby:
                try
                {
                    Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                    this._logger.Log($"Joined lobby {lobby.Name}");

                    if (!lobby.Data.TryGetValue(LOBBY_RELAY_CODE_KEY, out DataObject joinedLobbyData))
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
                }
                catch (RelayServiceException e) { this._logger.Log(e.ToString(), Logger.LogLevel.Error); }
                break;
            case MultiplayerState.WaitingForHostToStart:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
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
            this.ChangeState(MultiplayerState.WaitingForHostToStart);
        }
    }
}

public enum MultiplayerState
{
    None,
    NotConnected,
    Connected,
    CreatingLobby,
    HostWaitingForPlayers,
    JoiningLobby,
    WaitingForHostToStart,
    GameStarted,
}
