using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : NetworkBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _quitButton;

    public static bool IsPaused { get; private set; } = false;

    private CursorLockMode _prevCursorLockMode;
    private bool _prevCursorVisibility;

    private void Awake()
    {
        this._resumeButton.onClick.AddListener(this.ToggleOpen);
        this._quitButton.onClick.AddListener(this.OnQuitClick);
    }

    private void Start()
    {
        this._resumeButton.gameObject.SetActive(false);
        this._quitButton.gameObject.SetActive(false);
        this._canvas.enabled = false;
    }

    public override void OnDestroy()
    {
        this._resumeButton.onClick.RemoveListener(this.ToggleOpen);
        this._quitButton.onClick.RemoveListener(this.OnQuitClick);
        Time.timeScale = 1f;
        PauseMenuController.IsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.ToggleOpen();
        }
    }

    private void ToggleOpen()
    {
        if (PauseMenuController.IsPaused)
        {
            // Resuming game
            Cursor.lockState = this._prevCursorLockMode;
            Cursor.visible = this._prevCursorVisibility;

            if (!MultiplayerSystem.IsMultiplayer)
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            // Pausing game
            this._prevCursorLockMode = Cursor.lockState;
            this._prevCursorVisibility = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!MultiplayerSystem.IsMultiplayer)
            {
                Time.timeScale = 0f;
            }
        }

        PauseMenuController.IsPaused = !PauseMenuController.IsPaused;
        this._canvas.enabled = PauseMenuController.IsPaused;
        this._resumeButton.gameObject.SetActive(PauseMenuController.IsPaused);
        this._quitButton.gameObject.SetActive(PauseMenuController.IsPaused);
    }

    private void OnQuitClick()
    {
        NetworkManager.Singleton.Shutdown();
        MultiplayerSystem.Instance.ChangeState(MultiplayerState.Connected);
        SceneManager.LoadScene("MainMenuScene");
    }
}
