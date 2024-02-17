using Unity.Netcode;
using UnityEngine;

public class ActivateAbilityManager : NetworkBehaviour
{
    private SelectAbilityManager _selectAbilityManager;
    private SoldierDeathController _deathController;

    private NetworkVariable<bool> _isAbilityActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool IsAbilityActive => this._isAbilityActive.Value;

    private void Awake()
    {
        this._selectAbilityManager = GetComponent<SelectAbilityManager>();
        this._deathController = GetComponent<SoldierDeathController>();
        this._deathController.OnDeath += this.OnDeath;
        this._isAbilityActive.OnValueChanged += this.OnIsAbilityActiveChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._deathController.OnDeath -= this.OnDeath;
        this._isAbilityActive.OnValueChanged -= this.OnIsAbilityActiveChanged;
    }

    private void Update()
    {
        if (!this.IsOwner || !Input.GetKeyDown(KeyCode.Q)) { return; }
        if (this._selectAbilityManager.SelectedAbilityController == null)
        {
            Debug.LogWarning("Player has no selected ability!");
            return;
        }

        if (!this._selectAbilityManager.SelectedAbilityController.IsActive && this._selectAbilityManager.SelectedAbilityController.CanActivate())
            this.ActiveAbility();
        else if (this._selectAbilityManager.SelectedAbilityController.IsActive)
            this.DeactiveAbility();
    }

    private void ActiveAbility()
    {
        if (this._selectAbilityManager.SelectedAbilityController == null)
        {
            Debug.LogWarning("Player has no selected ability!");
            return;
        }

        this._selectAbilityManager.SelectedAbilityController.Activate();

        if (this.IsOwner)
            this._isAbilityActive.Value = true;
    }

    private void DeactiveAbility()
    {
        if (this._selectAbilityManager.SelectedAbilityController == null)
        {
            Debug.LogWarning("Player has no selected ability!");
            return;
        }

        this._selectAbilityManager.SelectedAbilityController.Deactivate();

        if (this.IsOwner)
            this._isAbilityActive.Value = false;
    }

    private void OnIsAbilityActiveChanged(bool _, bool isActive)
    {
        if (this.IsOwner) { return; }

        if (isActive)
            this.ActiveAbility();
        else
            this.DeactiveAbility();
    }

    private void OnDeath(ulong _)
    {
        if (this._selectAbilityManager.SelectedAbilityController == null) { return; }

        if (this._selectAbilityManager.SelectedAbilityController != null && this._selectAbilityManager.SelectedAbilityController.IsActive)
            this.DeactiveAbility();
    }
}
