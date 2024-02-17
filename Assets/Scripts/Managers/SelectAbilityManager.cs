using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SelectAbilityManager : NetworkBehaviour
{
    private ActivateAbilityManager _activateAbilityManager;

    [SerializeField] private Abilities _abilityOnSpawn;
    private AbilityController[] _allAbilities;
    private NetworkVariable<Abilities> _selectedAbility = new(Abilities.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Abilities SelectedAbility => this._selectedAbility.Value;
    public AbilityController SelectedAbilityController { get; private set; }
    public event Action<Abilities> OnLocalPlayerSelectedAbilityChanged;

    private void Awake()
    {
        this._activateAbilityManager = GetComponent<ActivateAbilityManager>();
        this._allAbilities = GetComponents<AbilityController>();
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
            this.SelectAbility(this._abilityOnSpawn);
        else
            this.SelectAbility(this.SelectedAbility);
    }

    private void Update()
    {
        if (!this.IsOwner || !this.IsKeyDownForAnAbility(out int abilityIndex)) { return; }
        if (this._activateAbilityManager.IsAbilityActive)
        {
            Debug.LogWarning("Can't switch abilities while one is active!");
            return;
        }

        this.SelectAbility((Abilities)abilityIndex);
    }

    private void SelectAbility(Abilities ability)
    {
        if (this.IsOwner && this._selectedAbility.Value == ability) { return; }

        this.SelectedAbilityController = this._allAbilities.FirstOrDefault(controller => controller.Ability == ability);

        if (this.IsOwner)
            this._selectedAbility.Value = ability;
    }

    private void OnSelectedAbilityChanged(Abilities _, Abilities newAbility)
    {
        if (this.IsOwner)
        {
            this.OnLocalPlayerSelectedAbilityChanged?.Invoke(newAbility);
            return;
        }

        this.SelectAbility(newAbility);
    }

    private bool IsKeyDownForAnAbility(out int abilityIndex)
    {
        abilityIndex = 0;

        for (int i = 1; i <= this._allAbilities.Length; i++)
        {
            if (!Input.GetKeyDown(KeyCode.Alpha0 + i)) { continue; }

            abilityIndex = i;
            return true;
        }

        return false;
    }
}
