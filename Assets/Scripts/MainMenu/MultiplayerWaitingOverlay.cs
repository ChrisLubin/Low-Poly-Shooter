using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerWaitingOverlay : MonoBehaviour
{
    [SerializeField] private Image _overlay;
    private TextMeshProUGUI _overlayText;

    private void Awake()
    {
        MultiplayerSystem.OnStateChange += this.OnMultiplayerStateChanged;
        this._overlayText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy() => MultiplayerSystem.OnStateChange -= this.OnMultiplayerStateChanged;

    private void OnMultiplayerStateChanged(MultiplayerState state)
    {
        switch (state)
        {
            case MultiplayerState.HostWaitingForPlayer:
                this._overlay.gameObject.SetActive(true);
                this._overlayText.text = "Waiting for a player to join...";
                break;
            case MultiplayerState.PlayerJoiningGame:
                this._overlay.gameObject.SetActive(true);
                this._overlayText.text = "Joining game...";
                break;
            case MultiplayerState.TwoPlayersConnected:
                this._overlay.gameObject.SetActive(false);
                this._overlayText.gameObject.SetActive(false);
                break;
        }
    }
}
