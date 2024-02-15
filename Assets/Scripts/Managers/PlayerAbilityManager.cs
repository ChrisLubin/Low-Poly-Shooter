using System.Linq;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    [SerializeField] private Abilities _abilityToEquip;
    private AbilityController _equippedAbility;

    private void Start()
    {
        this._equippedAbility = GetComponents<AbilityController>().FirstOrDefault(controller => controller.Ability == this._abilityToEquip);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (this._equippedAbility == null)
            {
                Debug.LogWarning("Player has no equipped ability!");
                return;
            }

            if (!this._equippedAbility.IsActive && this._equippedAbility.CanActivate())
                this._equippedAbility.Activate();
            else if (this._equippedAbility.IsActive)
                this._equippedAbility.Deactivate();
        }
    }
}
