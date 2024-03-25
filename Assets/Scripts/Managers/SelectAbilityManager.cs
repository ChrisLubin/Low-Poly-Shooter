using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SelectAbilityManager : NetworkBehaviorAutoDisableWithLogger<SelectAbilityManager>
{
    private ActivateAbilityManager _activateAbilityManager;

    private static Abilities _lastSelectedAbility;
    [SerializeField] private Abilities _abilityOnFirstSpawn;
    private NetworkVariable<Abilities> _selectedAbility = new(Abilities.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public AbilityController[] AllAbilities { get; private set; }
    public Abilities SelectedAbility => this._selectedAbility.Value;
    public AbilityController SelectedAbilityController { get; private set; }
    public static event Action<Abilities> OnLocalPlayerSelectedAbilityChanged;

    protected override void Awake()
    {
        base.Awake();
        this._activateAbilityManager = GetComponent<ActivateAbilityManager>();
        this.AllAbilities = GetComponents<AbilityController>();
        this._selectedAbility.OnValueChanged += this.OnSelectedAbilityChanged;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._selectedAbility.OnValueChanged -= this.OnSelectedAbilityChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (this.IsOwner)
        {
            if (_lastSelectedAbility == Abilities.None)
                this.SelectAbility(this._abilityOnFirstSpawn);
            else
                this.SelectAbility(_lastSelectedAbility);
        }
        else
            this.SelectAbility(this.SelectedAbility);
    }

    private void Update()
    {
        if (!this.IsOwner || PauseMenuController.IsPaused || SoldierKillStreakController.IS_USING_KILL_STREAK || !this.IsKeyDownForAnAbility(out int abilityIndex)) { return; }
        if (this._activateAbilityManager.IsAbilityActive)
        {
            this._logger.Log("Can't switch abilities while one is active!", Logger.LogLevel.Warning);
            return;
        }

        this.SelectAbility((Abilities)abilityIndex);
    }

    private void SelectAbility(Abilities ability)
    {
        if (this.IsOwner && this._selectedAbility.Value == ability) { return; }

        this.SelectedAbilityController = this.AllAbilities.FirstOrDefault(controller => controller.Ability == ability);

        if (this.IsOwner)
            this._selectedAbility.Value = ability;
    }

    private void OnSelectedAbilityChanged(Abilities _, Abilities newAbility)
    {
        if (this.IsOwner)
        {
            _lastSelectedAbility = newAbility;
            OnLocalPlayerSelectedAbilityChanged?.Invoke(newAbility);
            return;
        }

        this.SelectAbility(newAbility);
    }

    private bool IsKeyDownForAnAbility(out int abilityIndex)
    {
        abilityIndex = 0;

        for (int i = 1; i <= this.AllAbilities.Length; i++)
        {
            if (!Input.GetKeyDown(KeyCode.Alpha0 + i) && !Input.GetKeyDown(KeyCode.Keypad0 + i)) { continue; }

            abilityIndex = i;
            return true;
        }

        return false;
    }
}
