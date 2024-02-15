using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    private SoldierHealthController _healthController;
    private SoldierDamageController _damageController;
    private SoldierDeathController _deathController;

    [SerializeField] private Camera _playerCamera;
    [SerializeField] private AudioListener _audioListener;
    [SerializeField] private WeaponController _weaponController;

    public static event Action<ulong, SoldierController> OnSpawn;
    public static event Action<ulong, ulong> OnDeath;
    public static event Action<ulong> OnShoot;
    public static event Action<ulong, SoldierDamageController.DamageType, int> OnLocalTakeDamage;
    public static event Action<ulong, SoldierDamageController.DamageType, int> OnServerTakeDamage;
    public static event Action<ulong, SoldierDamageController.DamageType, int> OnServerDamageReceived;
    public static event Action<ulong, HealthData> OnHealthChange;

    private void Awake()
    {
        this._healthController = GetComponent<SoldierHealthController>();
        this._deathController = GetComponent<SoldierDeathController>();
        this._damageController = GetComponent<SoldierDamageController>();
        this._damageController.OnLocalTakeDamage += this._OnLocalTakeDamage;
        this._damageController.OnServerTakeDamage += this._OnServerTakeDamage;
        this._damageController.OnServerDamageReceived += this._OnServerDamageReceived;
        this._deathController.OnDeath += this._OnDeath;
        this._healthController.OnHealthChange += this._OnHealthChange;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SoldierController.OnSpawn?.Invoke(this.OwnerClientId, this);
    }

    protected override void OnOwnerNetworkSpawn()
    {
        this._playerCamera.enabled = true;
        this._audioListener.enabled = true;
        this._weaponController.OnShoot += this._OnShoot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weaponController.OnShoot -= this._OnShoot;
        this._damageController.OnLocalTakeDamage -= this._OnLocalTakeDamage;
        this._damageController.OnServerTakeDamage -= this._OnServerTakeDamage;
        this._damageController.OnServerDamageReceived -= this._OnServerDamageReceived;
        this._deathController.OnDeath -= this._OnDeath;
        this._healthController.OnHealthChange -= this._OnHealthChange;
    }

    public void TakeLocalDamage(SoldierDamageController.DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer) => this._damageController.TakeLocalDamage(type, damageAmount, damagePoint, isDamageFromLocalPlayer);
    public void TakeServerDamage(ulong damagerClientId, SoldierDamageController.DamageType type, int damageAmount) => this._damageController.TakeServerDamage(damagerClientId, type, damageAmount);
    private void _OnLocalTakeDamage(SoldierDamageController.DamageType type, int damageAmount) => SoldierController.OnLocalTakeDamage?.Invoke(this.OwnerClientId, type, damageAmount);
    private void _OnServerTakeDamage(ulong _, SoldierDamageController.DamageType type, int damageAmount) => SoldierController.OnServerTakeDamage?.Invoke(this.OwnerClientId, type, damageAmount);
    private void _OnServerDamageReceived(SoldierDamageController.DamageType type, int damageAmount) => SoldierController.OnServerDamageReceived?.Invoke(this.OwnerClientId, type, damageAmount);

    public void Shoot() => this._weaponController.Shoot();
    private void _OnShoot() => SoldierController.OnShoot?.Invoke(this.OwnerClientId);

    private void _OnDeath(ulong killerClientId) => SoldierController.OnDeath?.Invoke(this.OwnerClientId, killerClientId);
    private void _OnHealthChange(HealthData _, HealthData newHealthData) => SoldierController.OnHealthChange?.Invoke(this.OwnerClientId, newHealthData);
}
