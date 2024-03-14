using TMPro;
using UnityEngine;

public class KillStreakUIController : MonoBehaviour
{
    [SerializeField] private GameObject _uiContainer;
    [SerializeField] private TextMeshProUGUI _currentKillStreakCountText;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
        SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
        SoldierKillStreakController.OnLocalPlayerKillStreakCountChange += this.OnLocalPlayerKillStreakCountChange;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
        SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
        SoldierKillStreakController.OnLocalPlayerKillStreakCountChange -= this.OnLocalPlayerKillStreakCountChange;
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                this._uiContainer.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void OnLocalPlayerSpawn() => this._uiContainer.gameObject.SetActive(true);
    private void OnLocalPlayerDeath() => this._uiContainer.gameObject.SetActive(false);
    private void OnHostDisconnect() => this._uiContainer.gameObject.SetActive(false);
    private void OnLocalPlayerKillStreakCountChange(int currentKillStreakCount) => this._currentKillStreakCountText.text = $"{Mathf.Min(currentKillStreakCount, SoldierKillStreakController.KILLS_NEEDED_FOR_PREDATOR_MISSILE)}/{SoldierKillStreakController.KILLS_NEEDED_FOR_PREDATOR_MISSILE}";
}
