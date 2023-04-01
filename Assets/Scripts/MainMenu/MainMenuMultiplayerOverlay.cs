using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMultiplayerOverlay : MonoBehaviour
{
    [SerializeField] private Image _overlay;
    private TextMeshProUGUI _overlayStatusText;

    private void Awake()
    {
        MultiplayerSystem.OnStateChange += this.OnMultiplayerStateChanged;
        this._overlayStatusText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy() => MultiplayerSystem.OnStateChange -= this.OnMultiplayerStateChanged;

    private void OnMultiplayerStateChanged(MultiplayerState state)
    {
        switch (state)
        {
            case MultiplayerState.CreatingLobby:
                this._overlay.gameObject.SetActive(true);
                this._overlayStatusText.text = "Creating lobby...";
                break;
            case MultiplayerState.JoiningLobby:
                this._overlay.gameObject.SetActive(true);
                this._overlayStatusText.text = "Joining lobby...";
                break;
        }
    }
}
