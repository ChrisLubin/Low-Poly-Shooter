using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMultiplayerOverlay : MonoBehaviour
{
    [SerializeField] private Image _overlay;
    [SerializeField] private TextMeshProUGUI _overlayStatusText;
    [SerializeField] private Button _okButton;

    private void Awake()
    {
        MultiplayerSystem.OnStateChange += this.OnMultiplayerStateChanged;
        MultiplayerSystem.OnError += this.OnMultiplayerError;
        this._okButton.onClick.AddListener(this.OnOkClick);
    }

    private void OnDestroy()
    {
        MultiplayerSystem.OnStateChange -= this.OnMultiplayerStateChanged;
        MultiplayerSystem.OnError -= this.OnMultiplayerError;
        this._okButton.onClick.RemoveListener(this.OnOkClick);
    }

    private void OnMultiplayerStateChanged(MultiplayerState state)
    {
        switch (state)
        {
            case MultiplayerState.CreatingLobby:
                this._overlayStatusText.gameObject.SetActive(true);
                this._overlay.gameObject.SetActive(true);
                this._overlayStatusText.text = "Creating Lobby...";
                break;
            case MultiplayerState.JoiningLobby:
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
                this._okButton.gameObject.SetActive(true);
                break;
            case MultiplayerState.JoiningLobby:
                this._overlayStatusText.text = "Unable to Join Lobby";
                this._okButton.gameObject.SetActive(true);
                break;
        }
    }

    private void OnOkClick()
    {
        MultiplayerSystem.QuitMultiplayer();
        this._overlayStatusText.gameObject.SetActive(false);
        this._overlay.gameObject.SetActive(false);
        this._okButton.gameObject.SetActive(false);
    }
}
