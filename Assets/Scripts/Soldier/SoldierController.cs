using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    private SoldierDamageController _damageController;
    private SoldierDeathController _deathController;

    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private WeaponController _weaponController;

    public static Action<ulong, SoldierController> OnSpawn;
    public static Action<ulong> OnDeath;
    public static Action<ulong> OnShoot;
    public static Action<ulong, SoldierDamageController.DamageType, int> OnDamageReceived;

    private void Awake()
    {
        this._deathController = GetComponent<SoldierDeathController>();
        this._damageController = GetComponent<SoldierDamageController>();
        this._damageController.OnDamageReceived += this._OnDamageReceived;
        this._deathController.OnDeath += this._OnDeath;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SoldierController.OnSpawn?.Invoke(this.OwnerClientId, this);
    }

    protected override void OnOwnerNetworkSpawn()
    {
        this._playerCamera.GetComponent<Camera>().enabled = true;
        this._weaponController.OnShoot += this._OnShoot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weaponController.OnShoot -= this._OnShoot;
        this._damageController.OnDamageReceived -= this._OnDamageReceived;
    }

    public void TakeLocalDamage(SoldierDamageController.DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer) => this._damageController.TakeLocalDamage(type, damageAmount, damagePoint, isDamageFromLocalPlayer);
    public void TakeServerDamage(SoldierDamageController.DamageType type, int damageAmount) => this._damageController.TakeServerDamage(type, damageAmount);
    private void _OnDamageReceived(SoldierDamageController.DamageType type, int damageAmount) => SoldierController.OnDamageReceived?.Invoke(this.OwnerClientId, type, damageAmount);

    public void Shoot() => this._weaponController.Shoot();
    private void _OnShoot() => SoldierController.OnShoot?.Invoke(this.OwnerClientId);

    private void _OnDeath() => SoldierController.OnDeath?.Invoke(this.OwnerClientId);
}
