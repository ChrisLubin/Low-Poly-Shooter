using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityUIController : WithLogger<PlayerAbilityUIController>
{
    [SerializeField] private GameObject _uiContainer;
    [SerializeField] private TextMeshProUGUI _selectedAbilityText;
    [SerializeField] private Color _notSelectedOutlineColor;
    [SerializeField] private Color _selectedOutlineColor;
    [SerializeField] private Color _activatedOutlineColor;

    [SerializedDictionary("Ability", "Outline")] public AYellowpaper.SerializedCollections.SerializedDictionary<Abilities, Outline> AbilityOutlineMap; // Can't make private with serialize dictionary asset

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnStateChange += this.OnGameStateChange;
        MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
        SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
        SelectAbilityManager.OnLocalPlayerSelectedAbilityChanged += this.OnLocalPlayerSelectedAbilityChanged;
        ActivateAbilityManager.OnLocalPlayerAbilityActivatedOrDeactivated += OnLocalPlayerAbilityActivatedOrDeactivated;
        SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated += this.OnLocalPlayerKillStreakActivatedOrDeactivated;
    }

    private void Start()
    {
        this.ResetOutlines();
    }

    private void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
        SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
        SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
        SelectAbilityManager.OnLocalPlayerSelectedAbilityChanged -= this.OnLocalPlayerSelectedAbilityChanged;
        ActivateAbilityManager.OnLocalPlayerAbilityActivatedOrDeactivated -= OnLocalPlayerAbilityActivatedOrDeactivated;
        SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated -= this.OnLocalPlayerKillStreakActivatedOrDeactivated;
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                this._uiContainer.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void OnLocalPlayerSelectedAbilityChanged(Abilities newAbility)
    {
        this.ResetOutlines();

        this._selectedAbilityText.text = Helpers.SplitCamelCase(newAbility.ToString());
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

    private void OnLocalPlayerSpawn() => this._uiContainer.gameObject.SetActive(true);
    private void OnLocalPlayerDeath() => this._uiContainer.gameObject.SetActive(false);
    private void OnHostDisconnect() => this._uiContainer.gameObject.SetActive(false);
    private void OnLocalPlayerKillStreakActivatedOrDeactivated(bool wasActivated) => this._uiContainer.gameObject.SetActive(!wasActivated);

    private void OnLocalPlayerAbilityActivatedOrDeactivated(bool wasActivated, Abilities ability)
    {
        if (this.AbilityOutlineMap.TryGetValue(ability, out Outline outline))
            outline.effectColor = wasActivated ? this._activatedOutlineColor : this._selectedOutlineColor;
        else
            this._logger.Log($"Could not find UI for {ability} ability!", Logger.LogLevel.Warning);
    }
}
