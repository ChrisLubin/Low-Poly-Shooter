using TMPro;
using UnityEngine;

public class PredatorMissileUIController : MonoBehaviour
{
    [SerializeField] private GameObject _timerContainer;
    [SerializeField] private TextMeshProUGUI _timer;

    private void Update()
    {
        this._timerContainer.SetActive(SoldierKillStreakController.IS_USING_KILL_STREAK);

        if (SoldierKillStreakController.IS_USING_KILL_STREAK)
            this._timer.text = Mathf.Max(0f, PredatorMissileMovementController.AUTO_EXPLODE_TIME - PredatorMissileMovementController.AUTO_EXPLODE_TIMER).ToString("0.00");
    }
}
