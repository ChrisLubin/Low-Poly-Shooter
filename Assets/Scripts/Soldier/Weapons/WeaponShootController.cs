using System;
using UnityEngine;

public class WeaponShootController : NetworkBehaviorAutoDisable<WeaponShootController>
{
    [SerializeField] private Transform _bulletPrefab;
    [SerializeField] private Transform _bulletVfxPrefab;
    [SerializeField] private Transform _shootPoint;
    private float _bulletSpeed;

    private float _minTimeBetweenShots;
    private float _timeSinceLastShot = Mathf.Infinity;
    public Action OnShot;
    public Action OnLocalShot;

    public void Init(float bulletSpeed, int roundPerMinute)
    {
        int millisecondsInAMinute = 1000 * 60;
        this._minTimeBetweenShots = millisecondsInAMinute / roundPerMinute;
        this._bulletSpeed = bulletSpeed;
    }

    private void Update()
    {
        this._timeSinceLastShot += Time.deltaTime * 1000;
        if (Input.GetMouseButton(0) && this._timeSinceLastShot > this._minTimeBetweenShots)
        {
            Shoot(false);
            this._timeSinceLastShot = 0f;
        }
    }

    private void OnShootRpc() => this.Shoot(true);

    public void Shoot(bool isRpc)
    {
        Transform bullet = Instantiate(this._bulletPrefab, this._shootPoint.position, Quaternion.LookRotation(this._shootPoint.forward));
        Transform bulletVfx = Instantiate(this._bulletVfxPrefab, this._shootPoint.position, Quaternion.LookRotation(this._shootPoint.forward), transform);
        bullet.GetComponent<BulletController>().Init(this._bulletSpeed);
        this.OnShot?.Invoke();

        if (isRpc) { return; }
        this.OnLocalShot?.Invoke();
    }
}
