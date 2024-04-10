using System;
using InfimaGames.Animated.ModernGuns;
using UnityEngine;

public class WeaponController : NetworkBehaviorAutoDisable<WeaponController>
{
    private WeaponBehaviour _weapon;
    private WeaponShootController _shootController;
    private WeaponAmmoController _ammoController;

    [Header("Weapon Attributes")]
    [SerializeField] private int _bulletDamage = 15;
    [SerializeField] private float _bloomMaxAngle = 20f;
    [SerializeField] private int _magazineSize = 30;

    [field: SerializeField, Header("Recoil")] public float RecoilX { get; private set; } = -2f;
    [field: SerializeField] public float RecoilY { get; private set; } = 2f;
    [field: SerializeField] public float RecoilZ { get; private set; } = 0.35f;
    [field: SerializeField] public float Snappiness { get; private set; } = 6f;
    [field: SerializeField] public float ReturnSpeed { get; private set; } = 6f;

    private bool _isADS = false;
    public bool IsADS => this._isADS;
    public event Action<bool> OnADS;

    public event Action OnShoot;

    private void Awake()
    {
        this._weapon = GetComponent<WeaponBehaviour>();
        this._shootController = GetComponent<WeaponShootController>();
        this._ammoController = GetComponent<WeaponAmmoController>();
        this._weapon.OnShoot += this.OnShoot;
        this._shootController.Init(this._bulletDamage, this._bloomMaxAngle);
        this._ammoController.Init(this._magazineSize);
    }

    protected override void OnOwnerNetworkSpawn() => this._weapon.OnShoot += this.OnShoot;

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weapon.OnShoot -= this.OnShoot;
    }

    private void Update()
    {
        if (PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || SoldierKillStreakController.IS_USING_KILL_STREAK) { return; }

        bool wasADS = this._isADS;
        this._isADS = Input.GetMouseButton(1);

        if (wasADS == this._isADS) { return; }
        this.OnADS?.Invoke(this._isADS);
    }

    public void Shoot() => this._shootController.Shoot();
}
