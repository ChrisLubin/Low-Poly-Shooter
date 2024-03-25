using TMPro;
using UnityEngine;

public class PlayerAmmoUIController : MonoBehaviour
{
    [SerializeField] private GameObject _uiContainer;
    [SerializeField] private TextMeshProUGUI _ammoCountText;
    [SerializeField] private TextMeshProUGUI _magazineSizeText;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
        SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
        SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated += this.OnLocalPlayerKillStreakActivatedOrDeactivated;
        WeaponAmmoController.OnLocalPlayerAmmoChange += this.OnLocalPlayerAmmoChange;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
        SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
        SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated -= this.OnLocalPlayerKillStreakActivatedOrDeactivated;
        WeaponAmmoController.OnLocalPlayerAmmoChange -= this.OnLocalPlayerAmmoChange;
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
    private void OnLocalPlayerKillStreakActivatedOrDeactivated(bool wasActivated) => this._uiContainer.gameObject.SetActive(!wasActivated);

    private void OnLocalPlayerAmmoChange(int ammoCount, int magazineSize)
    {
        this._ammoCountText.text = $"<color={(((float)ammoCount / (float)magazineSize) <= 0.34f ? "red" : "white")}>{ammoCount}</color>";
        this._magazineSizeText.text = magazineSize.ToString();
    }
}
