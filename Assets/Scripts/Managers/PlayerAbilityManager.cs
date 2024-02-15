using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilityManager : NetworkBehaviorAutoDisable<PlayerAbilityManager>
{
    [SerializeField] private Abilities _abilityOnSpawn;
    private AbilityController[] _allAbilities;
    private AbilityController _equippedAbilityController;
    private NetworkVariable<Abilities> _equippedAbility = new(Abilities.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> _isAbilityActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        this._allAbilities = GetComponents<AbilityController>();
        this._equippedAbility.OnValueChanged += this.OnAbilityChanged;
        this._isAbilityActive.OnValueChanged += this.OnAbilityActiveChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._equippedAbility.OnValueChanged -= this.OnAbilityChanged;
        this._isAbilityActive.OnValueChanged -= this.OnAbilityActiveChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (this.IsOwner)
            this.EquipAbility(this._abilityOnSpawn);
        else
            this.EquipAbility(this._equippedAbility.Value);
    }

    private void Update()
    {
        if (!this.IsOwner || !Input.GetKeyDown(KeyCode.Q)) { return; }

        if (this._equippedAbilityController == null)
        {
            Debug.LogWarning("Player has no equipped ability!");
            return;
        }

        if (!this._equippedAbilityController.IsActive && this._equippedAbilityController.CanActivate())
            this.ActiveAbility();
        else if (this._equippedAbilityController.IsActive)
            this.DeactiveAbility();
    }

    private void EquipAbility(Abilities ability)
    {
        if (this._equippedAbility.Value != ability && this._equippedAbilityController != null && this._equippedAbilityController.IsActive)
            this.DeactiveAbility();

        this._equippedAbilityController = this._allAbilities.FirstOrDefault(controller => controller.Ability == ability);

        if (this.IsOwner)
            this._equippedAbility.Value = ability;
    }

    private void OnAbilityChanged(Abilities _, Abilities newAbility)
    {
        if (this.IsOwner) { return; }

        this.EquipAbility(newAbility);
    }

    private void ActiveAbility()
    {
        if (this._equippedAbilityController == null)
        {
            Debug.LogWarning("Player has no equipped ability!");
            return;
        }

        this._equippedAbilityController.Activate();

        if (this.IsOwner)
            this._isAbilityActive.Value = true;
    }

    private void DeactiveAbility()
    {
        if (this._equippedAbilityController == null)
        {
            Debug.LogWarning("Player has no equipped ability!");
            return;
        }

        this._equippedAbilityController.Deactivate();

        if (this.IsOwner)
            this._isAbilityActive.Value = false;
    }

    private void OnAbilityActiveChanged(bool _, bool isActive)
    {
        if (this.IsOwner) { return; }

        if (isActive)
            this.ActiveAbility();
        else
            this.DeactiveAbility();
    }
}
