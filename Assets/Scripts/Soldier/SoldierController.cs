using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private WeaponShootController _shootController;

    public static Action<SoldierController> OnLocalPlayerSpawn;
    public static Action<SoldierController> OnSpawn;
    public static Action<SoldierController> OnDespawn;
    public static Action<SoldierController> OnShot;
    public static Action<SoldierController, DamageType> OnDamageReceived;

    [Serializable]
    public enum DamageType
    {
        Bullet,
        Grenade,
        Missile
    }

    private void Awake() => SoldierController.OnSpawn?.Invoke(this);

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
    }

    public void TakeLocalDamage(DamageType type)
    {
        // Only take damage when another client says we have
        if (this.IsOwner) { return; }

        if (type == DamageType.Bullet)
        {
            // We shot another soldier locally
            SoldierController.OnDamageReceived?.Invoke(this, type);
        }
    }

    public void TakeServerDamage(DamageType type)
    {
        // Only take damage when another client says we have
        if (!this.IsOwner) { return; }

        if (type == DamageType.Bullet)
        {
            // We got shot by another client
            SoldierController.OnDamageReceived?.Invoke(this, type);
        }
    }

    public void Shoot() => this._shootController.Shoot();
    private void _OnShot() => SoldierController.OnShot?.Invoke(this);
}
