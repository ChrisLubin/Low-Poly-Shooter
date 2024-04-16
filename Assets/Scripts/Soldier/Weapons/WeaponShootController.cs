using System;
using InfimaGames.Animated.ModernGuns;
using Unity.Netcode;
using UnityEngine;

public class WeaponShootController : NetworkBehaviorAutoDisable<WeaponShootController>
{
    private Character _charater;

    private Transform _shootPoint;

    private const float _GUN_SHOT_AUDIO_VOLUME = 0.15f;
    private const float _BULLET_BLOOM_OFFSET = 0.1f;
    private int _bulletDamage;
    private float _bloomMaxAngle;

    public event Action OnShoot;

    public void Init(int bulletDamage, float bloomMaxAngle)
    {
        this._bulletDamage = bulletDamage;
        this._bloomMaxAngle = bloomMaxAngle;
    }

    private void Awake()
    {
        this._charater = GetComponentInParent<Character>();
        this._charater.OnShoot += this.Shoot;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        this._shootPoint = GetComponentInChildren<Muzzle>().GetShootPoint();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._charater.OnShoot -= this.Shoot;
    }

    public void Shoot()
    {
        // Vector3 pointForBulletToLookAt = this._isADS.Value ? this._shootPoint.position + this._shootPoint.forward : this.GetRandomBulletDirectionPoint(this._shootPoint.position, _BULLET_BLOOM_OFFSET, this._bloomMaxAngle, this._shootPoint.forward);

        // ObjectPoolSystem.Instance.TryGetObject(ObjectPoolSystem.PoolType.Bullet, out Transform bullet);
        // bullet.position = this._shootPoint.transform.position;
        // bullet.LookAt(pointForBulletToLookAt);
        // bullet.GetComponent<BulletController>().Init(this._bulletSpeed, this._bulletDamage, this.IsOwner);

        ObjectPoolSystem.Instance.TryGetObject(ObjectPoolSystem.PoolType.MuzzleFlash, out Transform muzzleFlash);
        muzzleFlash.transform.position = this._shootPoint.position;
        muzzleFlash.rotation = Quaternion.LookRotation(this._shootPoint.forward);
        muzzleFlash.GetComponent<MuzzleFlashController>().Init(this._shootPoint);

        // AudioSource.PlayClipAtPoint(this._gunShotAudioClip, this._shootPoint.position, _GUN_SHOT_AUDIO_VOLUME);
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
