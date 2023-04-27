using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    private SoldierDamageController _damageController;
    private SoldierDeathController _deathController;

    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private WeaponController _weaponController;

    public static Action<SoldierController> OnLocalPlayerSpawn;
    public static Action<SoldierController> OnSpawn;
    public static Action<SoldierController> OnDeath;
    public static Action<SoldierController> OnShot;
    public static Action<SoldierController, SoldierDamageController.DamageType, int> OnDamageReceived;

    private void Awake()
    {
        SoldierController.OnSpawn?.Invoke(this);
        this._deathController = GetComponent<SoldierDeathController>();
        this._damageController = GetComponent<SoldierDamageController>();
        this._damageController.OnDamageReceived += this._OnDamageReceived;
        this._deathController.OnDeath += this._OnDeath;
    }

    protected override void OnOwnerNetworkSpawn()
    {
        SoldierController.OnLocalPlayerSpawn?.Invoke(this);
        this._playerCamera.GetComponent<Camera>().enabled = true;
        this._weaponController.OnShot += this._OnShot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weaponController.OnShot -= this._OnShot;
        this._damageController.OnDamageReceived -= this._OnDamageReceived;
    }

    public void TakeLocalDamage(SoldierDamageController.DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer) => this._damageController.TakeLocalDamage(type, damageAmount, damagePoint, isDamageFromLocalPlayer);
    public void TakeServerDamage(SoldierDamageController.DamageType type, int damageAmount) => this._damageController.TakeServerDamage(type, damageAmount);
    private void _OnDamageReceived(SoldierDamageController.DamageType type, int damageAmount) => SoldierController.OnDamageReceived?.Invoke(this, type, damageAmount);

    public void Shoot() => this._weaponController.Shoot();
    private void _OnShot() => SoldierController.OnShot?.Invoke(this);

    private void _OnDeath() => SoldierController.OnDeath?.Invoke(this);
}
