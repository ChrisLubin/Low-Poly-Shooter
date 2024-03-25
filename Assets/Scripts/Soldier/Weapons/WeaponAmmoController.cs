using UnityEngine;

public class WeaponAmmoController : NetworkBehaviorAutoDisable<WeaponAmmoController>
{
    private WeaponShootController _shootController;

    private int _magazineSize;
    private int _bulletsInMagazine;

    public bool HasBulletInMagazine => this._bulletsInMagazine > 0;

    public void Init(int magazineSize)
    {
        this._magazineSize = magazineSize;
        this._bulletsInMagazine = magazineSize;
    }

    private void Awake()
    {
        this._shootController = GetComponent<WeaponShootController>();
    }

    protected override void OnOwnerNetworkSpawn()
    {
        this._shootController.OnShoot += this.OnShoot;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (!this.IsOwner) { return; }

        this._shootController.OnShoot -= this.OnShoot;
    }

    private void Update()
    {
        if (!this.IsOwner || PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || SoldierKillStreakController.IS_USING_KILL_STREAK) { return; }

        if (Input.GetKeyDown(KeyCode.R))
            this.Reload();
    }

    private void OnShoot() => this._bulletsInMagazine -= 1;
    private void Reload() => this._bulletsInMagazine = this._magazineSize;
}
