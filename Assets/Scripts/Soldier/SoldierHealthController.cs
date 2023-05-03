using System;
using Unity.Netcode;

public class SoldierHealthController : NetworkBehaviorAutoDisable<SoldierHealthController>
{
    private SoldierDamageController _damageController;

    private const int _MAX_HEALTH = 100;
    public const int MIN_HEALTH = 0;
    private NetworkVariable<int> _currentHealth = new(_MAX_HEALTH, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public event Action<int> OnHealthChange;

    private void Awake()
    {
        this._damageController = GetComponent<SoldierDamageController>();
        this._currentHealth.OnValueChanged += this._OnHealthChange;
    }
    protected override void OnOwnerNetworkSpawn() => this._damageController.OnDamageReceived += this.OnDamageReceived;

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._damageController.OnDamageReceived -= this.OnDamageReceived;
        this._currentHealth.OnValueChanged -= this._OnHealthChange;
    }

    private void OnDamageReceived(SoldierDamageController.DamageType _, int damageAmount)
    {
        if (this._currentHealth.Value == MIN_HEALTH) { return; }

        this._currentHealth.Value = Math.Clamp(this._currentHealth.Value - damageAmount, MIN_HEALTH, _MAX_HEALTH);
    }
    private void _OnHealthChange(int _, int newHealth) => this.OnHealthChange?.Invoke(newHealth);
}
