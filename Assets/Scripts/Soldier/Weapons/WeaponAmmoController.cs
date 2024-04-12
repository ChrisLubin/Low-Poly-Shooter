using System;
using InfimaGames.Animated.ModernGuns;
using UnityEngine;

public class WeaponAmmoController : NetworkBehaviorAutoDisable<WeaponAmmoController>
{
    private Weapon _weapon;
    private WeaponShootController _shootController;

    [SerializeField] private AudioClip _emptyMagazineSoundEffect;
    private const float _EMPTY_MAGAZINE_AUDIO_VOLUME = 0.7f;

    private int _magazineSize;
    private int _bulletsInMagazine;

    public bool HasBulletInMagazine => this._bulletsInMagazine > 0;
    public bool CanReload => this._bulletsInMagazine != this._magazineSize && !this._weapon.IsReloading;
    public event Action OnReloadRequest;

    public static event Action<int, int> OnLocalPlayerAmmoChange;

    public void Init(int magazineSize)
    {
        this._magazineSize = magazineSize;
        this._bulletsInMagazine = magazineSize;
    }

    private void Awake()
    {
        this._weapon = GetComponent<Weapon>();
        this._shootController = GetComponent<WeaponShootController>();
    }

    protected override void OnOwnerNetworkSpawn()
    {
        WeaponAmmoController.OnLocalPlayerAmmoChange?.Invoke(this._bulletsInMagazine, this._magazineSize);
        this._weapon.OnShoot += this.OnShoot;
        this._weapon.OnReloadDone += this.OnReloadDone;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (!this.IsOwner) { return; }

        this._weapon.OnShoot -= this.OnShoot;
        this._weapon.OnReloadDone -= this.OnReloadDone;
    }

    private void Update()
    {
        if (!this.IsOwner || PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || SoldierKillStreakController.IS_USING_KILL_STREAK) { return; }

        if (Input.GetKeyDown(KeyCode.R) && this._bulletsInMagazine != this._magazineSize)
            this.RequestReload();
    }

    private void OnShoot() => this.SetBulletsInMagazine(this._bulletsInMagazine - 1);
    private void OnReloadDone() => this.SetBulletsInMagazine(this._magazineSize);
    private void RequestReload() => this.OnReloadRequest?.Invoke();

    private void SetBulletsInMagazine(int count)
    {
        if (count == this._bulletsInMagazine) { return; }

        this._bulletsInMagazine = count;
        WeaponAmmoController.OnLocalPlayerAmmoChange?.Invoke(this._bulletsInMagazine, this._magazineSize);

        if (this._bulletsInMagazine == 0)
        {
            Helpers.PlayClipAtPoint(this._emptyMagazineSoundEffect, Vector3.zero, _EMPTY_MAGAZINE_AUDIO_VOLUME, out AudioSource audioSource);
            audioSource.spatialBlend = 0f;

            this.RequestReload();
        }
    }
}
