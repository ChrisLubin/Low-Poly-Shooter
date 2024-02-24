using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : NetworkBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Slider _frameRateSettingSlider;
    [SerializeField] private TextMeshProUGUI _frameRateSettingValue;

    private bool _didHostDisconnect = false;
    public static bool IsPaused { get; private set; } = false;

    private CursorLockMode _prevCursorLockMode;
    private bool _prevCursorVisibility;

    private void Awake()
    {
        this._resumeButton.onClick.AddListener(this.ToggleOpen);
        this._quitButton.onClick.AddListener(this.OnQuitClick);
        MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
        this._frameRateSettingSlider.onValueChanged.AddListener(this.OnFrameRateSettingSliderValueChange);
    }

    private void Start()
    {
        this._resumeButton.gameObject.SetActive(false);
        this._quitButton.gameObject.SetActive(false);
        this._canvas.enabled = false;
        this._frameRateSettingSlider.value = Application.targetFrameRate;
    }

    public override void OnDestroy()
    {
        this._resumeButton.onClick.RemoveListener(this.ToggleOpen);
        this._quitButton.onClick.RemoveListener(this.OnQuitClick);
        MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
        this._frameRateSettingSlider.onValueChanged.RemoveListener(this.OnFrameRateSettingSliderValueChange);
        Time.timeScale = 1f;
        PauseMenuController.IsPaused = false;
    }

    private void Update()
    {
        if (this._didHostDisconnect) { return; }

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
        MultiplayerSystem.QuitMultiplayer();
        SceneManager.LoadScene("MainMenuScene");
    }

    private void OnHostDisconnect()
    {
        if (this.IsHost || GameManager.State == GameState.PlayerWaitingForHostToStart || GameManager.State == GameState.GameOver) { return; }

        this._didHostDisconnect = true;
        this._headerText.text = "The host has left the lobby. Please return to the main menu.";
        this._resumeButton.gameObject.SetActive(false);

        if (PauseMenuController.IsPaused) { return; }
        this.ToggleOpen();
    }

    private void OnFrameRateSettingSliderValueChange(float newValue)
    {
        Application.targetFrameRate = (int)newValue;
        this._frameRateSettingValue.text = newValue.ToString();
    }
}
