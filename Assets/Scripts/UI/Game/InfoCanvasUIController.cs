using UnityEngine;

public class InfoCanvasUIController : MonoBehaviour
{
    [SerializeField] private GameObject _container;

    private bool _didHostDisconnect = false;

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

        // Only shows when not pause and scoreboard isn't open
        this._container.SetActive(!PauseMenuController.IsPaused && !Input.GetKey(KeyCode.Tab) && GameManager.State != GameState.GameOver && GameManager.State == GameState.GameStarted);
    }

    private void OnHostDisconnect()
    {
        this._didHostDisconnect = true;
        this._container.SetActive(false);
    }
}
