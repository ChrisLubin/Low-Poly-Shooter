using UnityEngine;

public class InfoCanvasUIController : MonoBehaviour
{
    [SerializeField] private GameObject _mainContainer;
    [SerializeField] private GameObject _defaultText;
    [SerializeField] private GameObject _infoContainer;

    private bool _didHostDisconnect = false;
    private bool _isShowingDefaultText = true;

    private void Awake()
    {
        MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
    }

    private void OnDestroy()
    {
        MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
    }

    private void Update()
    {
        if (this._didHostDisconnect) { return; }
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            this._isShowingDefaultText = !this._isShowingDefaultText;

        // Only shows when not pause and scoreboard isn't open
        this._mainContainer.SetActive(!PauseMenuController.IsPaused && !Input.GetKey(KeyCode.Tab) && GameManager.State != GameState.GameOver && GameManager.State == GameState.GameStarted);

        this._defaultText.SetActive(this._isShowingDefaultText);
        this._infoContainer.SetActive(!this._isShowingDefaultText);
    }

    private void OnHostDisconnect()
    {
        this._didHostDisconnect = true;
        this._mainContainer.SetActive(false);
    }
}
