using System.Linq;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMultiplayerOverlay : MonoBehaviour
{
    [SerializeField] private Image _overlay;
    [SerializeField] private TextMeshProUGUI _overlayStatusText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TextMeshProUGUI _confirmButtonText;

    private const float _SHOW_CANCEL_BUTTON__TIMEOUT = 5f;
    private float _timeSinceAttemptingToJoinOrCreate = 0f;
    private MultiplayerState[] _multiplayerStatesToShowCancelButton = new[] { MultiplayerState.CreatingLobby, MultiplayerState.CreatedLobby, MultiplayerState.JoiningLobby, MultiplayerState.JoinedLobby };

    private void Awake()
    {
        MultiplayerSystem.OnStateChange += this.OnMultiplayerStateChanged;
        MultiplayerSystem.OnLobbyError += this.OnLobbyError;
        MultiplayerSystem.OnRelayError += this.OnRelayError;
        MultiplayerSystem.OnError += this.OnMultiplayerError;
        this._confirmButton.onClick.AddListener(this.OnConfirmButtonClick);
    }

    private void OnDestroy()
    {
        MultiplayerSystem.OnStateChange -= this.OnMultiplayerStateChanged;
        MultiplayerSystem.OnLobbyError -= this.OnLobbyError;
        MultiplayerSystem.OnRelayError -= this.OnRelayError;
        MultiplayerSystem.OnError -= this.OnMultiplayerError;
        this._confirmButton.onClick.RemoveListener(this.OnConfirmButtonClick);
    }

    private void Update()
    {
        if (!this._multiplayerStatesToShowCancelButton.Contains(MultiplayerSystem.State)) { return; }
        this._timeSinceAttemptingToJoinOrCreate += Time.deltaTime;

        if (this._timeSinceAttemptingToJoinOrCreate < _SHOW_CANCEL_BUTTON__TIMEOUT) { return; }
        this.SetConfirmButton(true, "Cancel");
    }

    private void OnMultiplayerStateChanged(MultiplayerState state)
    {
        switch (state)
        {
            case MultiplayerState.CreatingLobby:
                this._timeSinceAttemptingToJoinOrCreate = 0f;
                this.SetConfirmButton(false);
                this._overlayStatusText.gameObject.SetActive(true);
                this._overlay.gameObject.SetActive(true);
                this._overlayStatusText.text = "Creating Lobby...";
                break;
            case MultiplayerState.JoiningLobby:
                this._timeSinceAttemptingToJoinOrCreate = 0f;
                this.SetConfirmButton(false);
                this._overlayStatusText.gameObject.SetActive(true);
                this._overlay.gameObject.SetActive(true);
                this._overlayStatusText.text = "Joining Lobby...";
                break;
        }
    }

    private void OnMultiplayerError()
    {
        switch (MultiplayerSystem.State)
        {
            case MultiplayerState.CreatingLobby:
                this._overlayStatusText.text = "Unable to Create Lobby";
                this.SetConfirmButton(true, "Ok");
                break;
            case MultiplayerState.JoiningLobby:
                this._overlayStatusText.text = "Unable to Join Lobby";
                this.SetConfirmButton(true, "Ok");
                break;
        }
    }

    private void OnLobbyError(LobbyExceptionReason errorCode)
    {
        switch (errorCode)
        {
            case LobbyExceptionReason.NoOpenLobbies:
                this._overlayStatusText.text = "There Are No Available Lobbies";
                this.SetConfirmButton(true, "Ok");
                break;
            case LobbyExceptionReason.RateLimited:
                this._overlayStatusText.text = "You Are Trying To Join Too Fast... Slow Down.";
                this.SetConfirmButton(true, "Ok");
                break;
            default:
                this._overlayStatusText.text = "Unable to Join Lobby";
                this.SetConfirmButton(true, "Ok");
                break;
        }
    }

    private void OnRelayError(RelayExceptionReason errorCode)
    {
        switch (errorCode)
        {
            case RelayExceptionReason.JoinCodeNotFound:
                this._overlayStatusText.text = "Unable to Connect to Host";
                this.SetConfirmButton(true, "Ok");
                break;
            default:
                this._overlayStatusText.text = "Unable to Connect to Host";
                this.SetConfirmButton(true, "Ok");
                break;
        }
    }

    private void SetConfirmButton(bool isActive, string text = "")
    {
        if (this._confirmButton.gameObject.activeSelf == isActive && this._confirmButtonText.text == text) { return; }

        this._confirmButton.gameObject.SetActive(isActive);
        this._confirmButtonText.text = text;
    }

    private void OnConfirmButtonClick()
    {
        MultiplayerSystem.QuitMultiplayer();
        this._overlayStatusText.gameObject.SetActive(false);
        this._overlay.gameObject.SetActive(false);
        this._confirmButton.gameObject.SetActive(false);
    }
}
