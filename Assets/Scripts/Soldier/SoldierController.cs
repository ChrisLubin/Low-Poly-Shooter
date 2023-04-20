using System;
using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private WeaponShootController _shootController;

    public static Action<SoldierController> OnSpawn;
    public static Action<SoldierController> OnDespawn;
    public static Action<SoldierController> OnLocalShot;

    private void Awake() => SoldierController.OnSpawn?.Invoke(this);

    protected override void OnOwnerNetworkSpawn()
    {
        this._playerCamera.GetComponent<Camera>().enabled = true;
        this._shootController.OnLocalShot += this._OnLocalShot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        SoldierController.OnDespawn?.Invoke(this);
        this._shootController.OnLocalShot -= this._OnLocalShot;
    }

    public void Shoot(bool isRpc) => this._shootController.Shoot(isRpc);
    private void _OnLocalShot() => SoldierController.OnLocalShot?.Invoke(this);
}
