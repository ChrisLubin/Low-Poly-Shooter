using System;
using UnityEngine;

public class WeaponController : NetworkBehaviorAutoDisable<WeaponController>
{
    private WeaponShootController _shootController;

    [Header("Weapon Attributes")]
    [SerializeField] private int _roundsPerMinute = 700;
    [SerializeField] private float _bulletSpeed = 70f;
    [SerializeField] private float _bloomMaxAngle = 20f;

    [field: SerializeField, Header("Recoil")] public float RecoilX { get; private set; } = -2f;
    [field: SerializeField] public float RecoilY { get; private set; } = 2f;
    [field: SerializeField] public float RecoilZ { get; private set; } = 0.35f;
    [field: SerializeField] public float Snappiness { get; private set; } = 6f;
    [field: SerializeField] public float ReturnSpeed { get; private set; } = 6f;

    private bool _isADS = false;
    private float _minTimeBetweenADS = WeaponAnimationController.ADS_ANIMATION_TIME_MILLISECONDS;
    private float _timeSinceLastADS = Mathf.Infinity;
    public Action<bool> OnADS;

    public Action OnShot;

    private void Awake()
    {
        this._shootController = GetComponent<WeaponShootController>();
        this._shootController.Init(this._bulletSpeed, this._roundsPerMinute, this._bloomMaxAngle);
    }

    protected override void OnOwnerNetworkSpawn() => this._shootController.OnShot += this.OnShot;

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._shootController.OnShot -= this.OnShot;
    }

    private void Update()
    {
        this._timeSinceLastADS += Time.deltaTime * 1000;

        if (Input.GetMouseButtonDown(1) && this._timeSinceLastADS > this._minTimeBetweenADS)
        {
            this._timeSinceLastADS = 0f;
            this._isADS = !this._isADS;
            this.OnADS?.Invoke(this._isADS);
        }
    }
}
