using TMPro;
using UnityEngine;

public class SoldierDebugController : MonoBehaviour
{
    private SoldierHealthController _healthController;
    [SerializeField] private TextMeshPro _debugHealthText;

    private void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            this._debugHealthText.gameObject.SetActive(false);
            this.enabled = false;
            return;
        }

        this._debugHealthText.gameObject.SetActive(true);
        this._healthController = GetComponent<SoldierHealthController>();
        this._healthController.OnHealthChange += this.OnHealthChange;
    }

    private void OnDestroy() => this._healthController.OnHealthChange -= this.OnHealthChange;
    private void OnHealthChange(int newHealth) => this._debugHealthText.text = newHealth.ToString();
}
