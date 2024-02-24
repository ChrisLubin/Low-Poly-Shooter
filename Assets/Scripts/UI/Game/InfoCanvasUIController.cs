using UnityEngine;

public class InfoCanvasUIController : MonoBehaviour
{
    [SerializeField] private GameObject _container;

    private void Update()
    {
        // Only shows when not pause and scoreboard isn't open
        this._container.SetActive(!PauseMenuController.IsPaused && !Input.GetKey(KeyCode.Tab) && GameManager.State != GameState.GameOver && MultiplayerSystem.IsMultiplayer && GameManager.State == GameState.GameStarted);
    }
}
