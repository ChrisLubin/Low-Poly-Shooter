using Unity.Netcode;
using UnityEngine;

public class ActivateAbilityManager : NetworkBehaviorAutoDisableWithLogger<ActivateAbilityManager>
{
    private SelectAbilityManager _selectAbilityManager;
    private SoldierDeathController _deathController;

    private NetworkVariable<bool> _isAbilityActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool IsAbilityActive => this._isAbilityActive.Value;

    protected override void Awake()
    {
        base.Awake();
        this._selectAbilityManager = GetComponent<SelectAbilityManager>();
        this._deathController = GetComponent<SoldierDeathController>();
        this._deathController.OnDeath += this.OnDeath;
        this._isAbilityActive.OnValueChanged += this.OnIsAbilityActiveChanged;

        foreach (AbilityController ability in this._selectAbilityManager.AllAbilities)
            ability.OnInternallyDeactivated += this.OnAbilityInternallyDeactivated;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._deathController.OnDeath -= this.OnDeath;
        this._isAbilityActive.OnValueChanged -= this.OnIsAbilityActiveChanged;

        foreach (AbilityController ability in this._selectAbilityManager.AllAbilities)
            ability.OnInternallyDeactivated -= this.OnAbilityInternallyDeactivated;
    }

    private void Update()
    {
        if (!this.IsOwner || PauseMenuController.IsPaused || SoldierKillStreakController.IS_USING_KILL_STREAK || !Input.GetKeyDown(KeyCode.Q)) { return; }
        if (this._selectAbilityManager.SelectedAbilityController == null)
        {
            this._logger.Log("Player has no selected ability!", Logger.LogLevel.Warning);
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
            this._logger.Log("Player has no selected ability!", Logger.LogLevel.Warning);
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
            this._logger.Log("Player has no selected ability!", Logger.LogLevel.Warning);
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

    private void OnAbilityInternallyDeactivated()
    {
        if (!this.IsOwner) { return; }

        if (this._isAbilityActive.Value)
            this._isAbilityActive.Value = false;
    }
}
