using System;
using Unity.Netcode;

public class SoldierHealthController : NetworkBehaviour
{
    private SoldierDamageController _damageController;

    private const int _MAX_HEALTH = 100;
    public const int MIN_HEALTH = 0;
    private NetworkVariable<HealthData> _currentHealth = new(new(_MAX_HEALTH, MIN_HEALTH));
    public event Action<HealthData, HealthData> OnHealthChange;

    private void Awake()
    {
        this._damageController = GetComponent<SoldierDamageController>();
        this._currentHealth.OnValueChanged += this._OnHealthChange;
        this._damageController.OnServerTakeDamage += this.OnServerTakeDamage;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._currentHealth.OnValueChanged -= this._OnHealthChange;
        this._damageController.OnServerTakeDamage -= this.OnServerTakeDamage;
    }

    private void OnServerTakeDamage(SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (this._currentHealth.Value.Health == MIN_HEALTH || !this.IsHost) { return; }

        this._currentHealth.Value = this._currentHealth.Value.DecreaseHealth(damageAmount, damageType);
    }

    private void _OnHealthChange(HealthData oldHealthData, HealthData newHealthData) => this.OnHealthChange?.Invoke(oldHealthData, newHealthData);
}

[Serializable]
public struct HealthData : INetworkSerializable
{
    public int Health;
    public int _MAX_HEALTH;
    public int MIN_HEALTH;
    public SoldierDamageController.DamageType LatestDamageType;

    public HealthData(int maxHealth, int minHealth, SoldierDamageController.DamageType latestDamageType = SoldierDamageController.DamageType.Bullet)
    {
        this.Health = maxHealth;
        this._MAX_HEALTH = maxHealth;
        this.MIN_HEALTH = minHealth;
        this.LatestDamageType = latestDamageType;
    }

    public HealthData DecreaseHealth(int damageAmount, SoldierDamageController.DamageType damageType)
    {
        this.Health = Math.Clamp(this.Health - damageAmount, this.MIN_HEALTH, this._MAX_HEALTH);
        this.LatestDamageType = damageType;
        return this;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out Health);
            reader.ReadValueSafe(out LatestDamageType);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(Health);
            writer.WriteValueSafe(LatestDamageType);
        }
    }
}
