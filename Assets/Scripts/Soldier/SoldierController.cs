using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private SoldierDamageController _damageController;

    public static Action<SoldierController> OnLocalPlayerSpawn;
    public static Action<SoldierController> OnSpawn;
    public static Action<SoldierController> OnDespawn;
    public static Action<SoldierController> OnShot;
    public static Action<SoldierController, SoldierDamageController.DamageType, int> OnDamageReceived;

    private void Awake()
    {
        SoldierController.OnSpawn?.Invoke(this);
        this._damageController.OnDamageReceived += this._OnDamageReceived;
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
        SoldierController.OnDespawn?.Invoke(this);
        this._weaponController.OnShot -= this._OnShot;
        this._damageController.OnDamageReceived -= this._OnDamageReceived;
    }

    public void TakeLocalDamage(SoldierDamageController.DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer) => this._damageController.TakeLocalDamage(type, damageAmount, damagePoint, isDamageFromLocalPlayer);
    public void TakeServerDamage(SoldierDamageController.DamageType type, int damageAmount) => this._damageController.TakeServerDamage(type, damageAmount);
    private void _OnDamageReceived(SoldierDamageController.DamageType type, int damageAmount) => SoldierController.OnDamageReceived?.Invoke(this, type, damageAmount);

    public void Shoot() => this._weaponController.Shoot();
    private void _OnShot() => SoldierController.OnShot?.Invoke(this);
}
