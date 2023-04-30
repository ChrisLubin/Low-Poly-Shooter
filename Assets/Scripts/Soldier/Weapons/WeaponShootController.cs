using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponShootController : NetworkBehaviorAutoDisable<WeaponShootController>
{
    private WeaponController _weaponController;

    [SerializeField] private Transform _bulletPrefab;
    [SerializeField] private Transform _muzzleFlashVfxPrefab;
    [SerializeField] private Transform _shootPoint;

    private const float _BULLET_BLOOM_OFFSET = 0.1f;
    private float _bulletSpeed;
    private int _bulletDamage;
    private float _bloomMaxAngle;

    private float _minTimeBetweenShots;
    private float _timeSinceLastShot = Mathf.Infinity;
    public Action OnShoot;

    private NetworkVariable<bool> _isADS = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void Init(float bulletSpeed, int bulletDamage, int roundPerMinute, float bloomMaxAngle)
    {
        int millisecondsInAMinute = 1000 * 60;
        this._minTimeBetweenShots = millisecondsInAMinute / roundPerMinute;
        this._bulletSpeed = bulletSpeed;
        this._bulletDamage = bulletDamage;
        this._bloomMaxAngle = bloomMaxAngle;
    }

    private void Awake() => this._weaponController = GetComponent<WeaponController>();
    protected override void OnOwnerNetworkSpawn() => this._weaponController.OnADS += this.OnADS;

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weaponController.OnADS -= this.OnADS;
    }

    private void Update()
    {
        this._timeSinceLastShot += Time.deltaTime * 1000;
        if (Input.GetMouseButton(0) && this._timeSinceLastShot > this._minTimeBetweenShots)
        {
            Shoot();
            this._timeSinceLastShot = 0f;
        }
    }

    private void OnADS(bool isADS) => this._isADS.Value = isADS;

    public void Shoot()
    {
        Vector3 pointForBulletToLookAt = this._isADS.Value ? this._shootPoint.position + this._shootPoint.forward : this.GetRandomBulletDirectionPoint(this._shootPoint.position, _BULLET_BLOOM_OFFSET, this._bloomMaxAngle, this._shootPoint.forward);

        ObjectPoolSystem.Instance.TryGetObject(ObjectPoolSystem.PoolType.Bullet, out Transform bullet);
        bullet.position = this._shootPoint.transform.position;
        bullet.LookAt(pointForBulletToLookAt);
        bullet.GetComponent<BulletController>().Init(this._bulletSpeed, this._bulletDamage, this.IsOwner);

        ObjectPoolSystem.Instance.TryGetObject(ObjectPoolSystem.PoolType.MuzzleFlash, out Transform muzzleFlash);
        muzzleFlash.transform.position = this._shootPoint.position;
        muzzleFlash.rotation = Quaternion.LookRotation(this._shootPoint.forward);
        muzzleFlash.GetComponent<MuzzleFlashController>().Init();

        this.OnShoot?.Invoke();
    }

    public Vector3 GetRandomBulletDirectionPoint(Vector3 origin, float coneAltitude, float coneAngle, Vector3 coneDirection, float biasTowardsCenter = 1f)
    {
        // Convert cone angle to radians
        float coneAngleRad = Mathf.Deg2Rad * coneAngle;

        // Calculate the radius at the base of the cone
        float radius = coneAltitude * Mathf.Tan(coneAngleRad / 2f);

        // Get a random position within a unit circle, biased towards the center
        Vector2 randomCirclePoint = UnityEngine.Random.insideUnitCircle;
        randomCirclePoint *= Mathf.Pow(randomCirclePoint.magnitude, biasTowardsCenter);

        // Calculate the position on the base of the cone using the random position within the unit circle
        Vector3 basePoint = origin + coneDirection.normalized * coneAltitude;
        Vector3 offset = Quaternion.FromToRotation(Vector3.up, coneDirection.normalized) * (radius * new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y));
        basePoint += offset;

        return basePoint;
    }
}
