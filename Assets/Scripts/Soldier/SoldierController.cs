using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private WeaponShootController _shootController;
    [SerializeField] private SoldierDamageController _damageController;

    public static Action<SoldierController> OnLocalPlayerSpawn;
    public static Action<SoldierController> OnSpawn;
    public static Action<SoldierController> OnDespawn;
    public static Action<SoldierController> OnShot;
    public static Action<SoldierController, SoldierDamageController.DamageType> OnDamageReceived;

    private void Awake()
    {
        SoldierController.OnSpawn?.Invoke(this);
        this._damageController.OnDamageReceived += this._OnDamageReceived;
    }

    protected override void OnOwnerNetworkSpawn()
    {
        SoldierController.OnLocalPlayerSpawn?.Invoke(this);
        this._playerCamera.GetComponent<Camera>().enabled = true;
        this._shootController.OnShot += this._OnShot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        SoldierController.OnDespawn?.Invoke(this);
        this._shootController.OnShot -= this._OnShot;
        this._damageController.OnDamageReceived -= this._OnDamageReceived;
    }

    public void TakeLocalDamage(SoldierDamageController.DamageType type) => this._damageController.TakeLocalDamage(type);
    public void TakeServerDamage(SoldierDamageController.DamageType type) => this._damageController.TakeServerDamage(type);
    private void _OnDamageReceived(SoldierDamageController.DamageType type) => SoldierController.OnDamageReceived?.Invoke(this, type);

    public void Shoot() => this._shootController.Shoot();
    private void _OnShot() => SoldierController.OnShot?.Invoke(this);
}
