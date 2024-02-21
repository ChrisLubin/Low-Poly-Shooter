using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityUIController : WithLogger<PlayerAbilityUIController>
{
    [SerializeField] private Color _notSelectedOutlineColor;
    [SerializeField] private Color _selectedOutlineColor;

    [SerializedDictionary("Ability", "Outline")] public SerializedDictionary<Abilities, Outline> AbilityOutlineMap; // Can't make private with serialize dictionary asset

    protected override void Awake()
    {
        base.Awake();
        SelectAbilityManager.OnLocalPlayerSelectedAbilityChanged += this.OnLocalPlayerSelectedAbilityChanged;
    }

    private void Start()
    {
        this.ResetOutlines();
    }

    private void OnDestroy()
    {
        SelectAbilityManager.OnLocalPlayerSelectedAbilityChanged -= this.OnLocalPlayerSelectedAbilityChanged;
    }

    private void OnLocalPlayerSelectedAbilityChanged(Abilities newAbility)
    {
        this.ResetOutlines();

        if (this.AbilityOutlineMap.TryGetValue(newAbility, out Outline outline))
            outline.effectColor = this._selectedOutlineColor;
        else
            this._logger.Log($"Could not find UI for {newAbility} ability!", Logger.LogLevel.Warning);
    }

    private void ResetOutlines()
    {
        foreach (KeyValuePair<Abilities, Outline> pair in this.AbilityOutlineMap)
            pair.Value.effectColor = this._notSelectedOutlineColor;
    }
}
