using UnityEngine;
using UnityEngine.UI;

public class PlayerBloodUIController : MonoBehaviour
{
    [SerializeField] private Image _bloodOverlay;

    private const int _BLOOD_COLOR_INTENSITY = 145; // From 0 to 255
    private const float _MAX_BLOOD_OPACITY = 0.15f; // From 0 to 1

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        SoldierManager.OnLocalPlayerHealthChange += this.OnLocalPlayerHealthChange;
        SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
    }
    private void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        SoldierManager.OnLocalPlayerHealthChange -= this.OnLocalPlayerHealthChange;
        SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
    }

    private void OnLocalPlayerHealthChange(HealthData newHealthData)
    {
        if (GameManager.State == GameState.GameOver) { return; }

        float healthPercentage = (float)newHealthData.Health / SoldierHealthController.MAX_HEALTH;
        float overlayAlpha = Mathf.Abs((healthPercentage * _MAX_BLOOD_OPACITY) - _MAX_BLOOD_OPACITY);
        this._bloodOverlay.color = new(_BLOOD_COLOR_INTENSITY, 0, 0, overlayAlpha);
    }

    private void OnLocalPlayerSpawn() => this._bloodOverlay.color = new(_BLOOD_COLOR_INTENSITY, 0, 0, 0);

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                this._bloodOverlay.color = new(_BLOOD_COLOR_INTENSITY, 0, 0, 0);
                break;
            default:
                break;
        }
    }
}
