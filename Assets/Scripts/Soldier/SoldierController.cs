using System;
using InfimaGames.Animated.ModernGuns;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    private Character _character;
    private SoldierHealthController _healthController;
    private SoldierDamageController _damageController;
    private SoldierDeathController _deathController;
    private WeaponController _weaponController;

    public static event Action<ulong, SoldierController> OnSpawn;
    public static event Action<ulong, ulong, DamageType> OnDeath;
    public static event Action<ulong> OnShoot;
    public static event Action<ulong, Vector3, DamageType, int> OnLocalTakeDamage;
    public static event Action<ulong, Vector3, DamageType, int> OnServerTakeDamage;
    public static event Action<ulong, DamageType, int> OnServerDamageReceived;
    public static event Action<ulong, HealthData> OnHealthChange;

    private void Awake()
    {
        this._character = GetComponent<Character>();
        this._healthController = GetComponent<SoldierHealthController>();
        this._deathController = GetComponent<SoldierDeathController>();
        this._damageController = GetComponent<SoldierDamageController>();
        this._damageController.OnPlayerDamagedByLocalPlayer += this._OnLocalTakeDamage;
        this._damageController.OnServerTakeDamage += this._OnServerTakeDamage;
        this._damageController.OnServerDamageReceived += this._OnServerDamageReceived;
        this._deathController.OnDeath += this._OnDeath;
        this._healthController.OnHealthChange += this._OnHealthChange;
    }

    public override void OnNetworkSpawn()
    {
        this._weaponController = this._character.GetInventory().GetEquipped().GetComponent<WeaponController>();

        base.OnNetworkSpawn();
        SoldierController.OnSpawn?.Invoke(this.OwnerClientId, this);
    }

    protected override void OnOwnerNetworkSpawn()
    {
        this._weaponController.OnShoot += this._OnShoot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weaponController.OnShoot -= this._OnShoot;
        this._damageController.OnPlayerDamagedByLocalPlayer -= this._OnLocalTakeDamage;
        this._damageController.OnServerTakeDamage -= this._OnServerTakeDamage;
        this._damageController.OnServerDamageReceived -= this._OnServerDamageReceived;
        this._deathController.OnDeath -= this._OnDeath;
        this._healthController.OnHealthChange -= this._OnHealthChange;
    }

    public void TakeLocalDamage(DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer) => this._damageController.TakeLocalDamage(type, damageAmount, damagePoint, isDamageFromLocalPlayer);
    public void TakeServerDamage(ulong damagerClientId, Vector3 damagePoint, DamageType type, int damageAmount) => this._damageController.TakeServerDamage(damagerClientId, damagePoint, type, damageAmount);
    private void _OnLocalTakeDamage(Vector3 damagePoint, DamageType type, int damageAmount) => SoldierController.OnLocalTakeDamage?.Invoke(this.OwnerClientId, damagePoint, type, damageAmount);
    private void _OnServerTakeDamage(ulong _, Vector3 damagePoint, DamageType type, int damageAmount) => SoldierController.OnServerTakeDamage?.Invoke(this.OwnerClientId, damagePoint, type, damageAmount);
    private void _OnServerDamageReceived(DamageType type, int damageAmount) => SoldierController.OnServerDamageReceived?.Invoke(this.OwnerClientId, type, damageAmount);

    public void Shoot() => this._weaponController.Shoot();
    private void _OnShoot() => SoldierController.OnShoot?.Invoke(this.OwnerClientId);

    private void _OnDeath(ulong killerClientId, DamageType latestDamageType) => SoldierController.OnDeath?.Invoke(this.OwnerClientId, killerClientId, latestDamageType);
    private void _OnHealthChange(HealthData _, HealthData newHealthData) => SoldierController.OnHealthChange?.Invoke(this.OwnerClientId, newHealthData);
}
