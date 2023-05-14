using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierHealthController : NetworkBehaviour
{
    private SoldierDamageController _damageController;

    public const int MAX_HEALTH = 100;
    public const int MIN_HEALTH = 0;
    private NetworkVariable<HealthData> _currentHealth = new(new(MAX_HEALTH, MIN_HEALTH));

    public event Action<HealthData, HealthData> OnHealthChange;

    private float _timeSinceLastDamage = 0f;
    private float _timeSinceLastHealthRegeneration = 0f;
    private const float _START_HEALTH_REGENERATION_TIMEOUT = 5f;
    private const float _HEALTH_REGENERATION_INTERVAL = 0.234f;
    private const int _HEALTH_REGENERATION_AMOUNT = 10;

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

    private void Update()
    {
        if (!this.IsHost || this._currentHealth.Value.Health == MAX_HEALTH) { return; }
        this._timeSinceLastDamage += Time.deltaTime;
        this._timeSinceLastHealthRegeneration += Time.deltaTime;

        if (this._timeSinceLastDamage < _START_HEALTH_REGENERATION_TIMEOUT || this._timeSinceLastHealthRegeneration < _HEALTH_REGENERATION_INTERVAL) { return; }
        this._currentHealth.Value = this._currentHealth.Value.IncreaseHealth(_HEALTH_REGENERATION_AMOUNT);
        this._timeSinceLastHealthRegeneration = 0f;
    }

    private void OnServerTakeDamage(ulong damagerClientId, SoldierDamageController.DamageType damageType, int damageAmount)
    {
        if (this._currentHealth.Value.Health == MIN_HEALTH || !this.IsHost) { return; }

        this._currentHealth.Value = this._currentHealth.Value.DecreaseHealth(damagerClientId, damageAmount, damageType);
        this._timeSinceLastDamage = 0f;
        this._timeSinceLastHealthRegeneration = 0f;
    }

    private void _OnHealthChange(HealthData oldHealthData, HealthData newHealthData) => this.OnHealthChange?.Invoke(oldHealthData, newHealthData);
}

[Serializable]
public struct HealthData : INetworkSerializable
{
    public int Health;
    public int _MAX_HEALTH;
    public int MIN_HEALTH;
    public ulong LatestDamagerClientId;
    public SoldierDamageController.DamageType LatestDamageType;

    public HealthData(int maxHealth, int minHealth, SoldierDamageController.DamageType latestDamageType = SoldierDamageController.DamageType.Bullet)
    {
        this.Health = maxHealth;
        this._MAX_HEALTH = maxHealth;
        this.MIN_HEALTH = minHealth;
        this.LatestDamagerClientId = 0;
        this.LatestDamageType = latestDamageType;
    }

    public HealthData IncreaseHealth(int increaseAmount)
    {
        this.Health = Math.Clamp(this.Health + increaseAmount, this.MIN_HEALTH, this._MAX_HEALTH);
        return this;
    }

    public HealthData DecreaseHealth(ulong damagerClientId, int damageAmount, SoldierDamageController.DamageType damageType)
    {
        this.Health = Math.Clamp(this.Health - damageAmount, this.MIN_HEALTH, this._MAX_HEALTH);
        this.LatestDamagerClientId = damagerClientId;
        this.LatestDamageType = damageType;
        return this;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out Health);
            reader.ReadValueSafe(out LatestDamagerClientId);
            reader.ReadValueSafe(out LatestDamageType);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(Health);
            writer.WriteValueSafe(LatestDamagerClientId);
            writer.WriteValueSafe(LatestDamageType);
        }
    }
}
