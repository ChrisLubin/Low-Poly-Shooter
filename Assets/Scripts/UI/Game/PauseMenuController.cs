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
    [SerializeField] private Slider _mouseSensitivitySettingSlider;
    [SerializeField] private TextMeshProUGUI _mouseSensitivitySettingValue;
    [SerializeField] private Toggle _muteMusicToggle;

    public static bool IsPaused { get; private set; } = false;
    public static float MouseSensitivityMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        this._resumeButton.onClick.AddListener(this.ToggleOpen);
        this._quitButton.onClick.AddListener(this.OnQuitClick);
        this._frameRateSettingSlider.onValueChanged.AddListener(this.OnFrameRateSettingSliderValueChange);
        this._mouseSensitivitySettingSlider.onValueChanged.AddListener(this.OnMouseSensitivitySettingSliderValueChange);
        this._muteMusicToggle.onValueChanged.AddListener(this.OnMuteMusicToggleClick);
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
        this._frameRateSettingSlider.onValueChanged.RemoveListener(this.OnFrameRateSettingSliderValueChange);
        this._mouseSensitivitySettingSlider.onValueChanged.RemoveListener(this.OnMouseSensitivitySettingSliderValueChange);
        this._muteMusicToggle.onValueChanged.RemoveListener(this.OnMuteMusicToggleClick);
        Time.timeScale = 1f;
        PauseMenuController.IsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            this.ToggleOpen();

        this._muteMusicToggle.gameObject.SetActive(EndGameMusicManager.IsPlayingMusic());
        this._muteMusicToggle.SetIsOnWithoutNotify(EndGameMusicManager.IsMuted());
    }

    private void ToggleOpen()
    {
        if (PauseMenuController.IsPaused)
        {
            // Resuming game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!MultiplayerSystem.IsMultiplayer)
                Time.timeScale = 1f;
        }
        else
        {
            // Pausing game
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!MultiplayerSystem.IsMultiplayer)
                Time.timeScale = 0f;
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

    private void OnMuteMusicToggleClick(bool _) => EndGameMusicManager.ToggleMute();

    private void OnFrameRateSettingSliderValueChange(float newValue)
    {
        Application.targetFrameRate = (int)newValue;
        this._frameRateSettingValue.text = newValue.ToString();
    }

    private void OnMouseSensitivitySettingSliderValueChange(float newValue)
    {
        PauseMenuController.MouseSensitivityMultiplier = newValue;
        this._mouseSensitivitySettingValue.text = $"{newValue:0.00}x";
    }
}
