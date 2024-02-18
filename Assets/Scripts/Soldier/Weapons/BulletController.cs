using UnityEngine;

public class BulletController : MonoBehaviour
{
    private float _speed = 0f;
    private int _damageAmount = 0;
    private float _timeSinceCreation = 0f;
    private float _maxLifeSpan = 2f;
    private bool _wasShotByLocalPlayer;
    private Vector3 _scaleOnSpawn = new(1f, 1f, 0.05f);

    public void Init(float bulletSpeed, int damageAmount, bool wasShotByLocalPlayer)
    {
        this._timeSinceCreation = 0f;
        this._speed = bulletSpeed;
        this._damageAmount = damageAmount;
        this._wasShotByLocalPlayer = wasShotByLocalPlayer;
        this.transform.localScale = this._scaleOnSpawn;
    }

    private void Start()
    {
        if (this.WillCollideNextFrame(out Vector3 collidePosition, out IDamageable damageable, Constants.LayerNames.Damageable))
            this.OnWillCollide(collidePosition, damageable);
    }

    private void FixedUpdate()
    {
        if (this.WillCollideNextFrame(out Vector3 collidePosition, out IDamageable damageable, Constants.LayerNames.Damageable))
        {
            this.OnWillCollide(collidePosition, damageable);
            return;
        }

        this._timeSinceCreation += Time.fixedDeltaTime;
        transform.position = this.GetNextPosition();
        this.transform.localScale = Vector3.one;

        if (this._timeSinceCreation > this._maxLifeSpan)
            ObjectPoolSystem.Instance.ReleaseObject(ObjectPoolSystem.PoolType.Bullet, transform);
    }

    private Vector3 GetNextPosition() => transform.position + transform.forward * this._speed * Time.fixedDeltaTime;

    private bool WillCollideNextFrame<T>(out Vector3 collidePosition, out T collideObject, string layerName)
    {
        collidePosition = Vector3.zero;
        collideObject = default(T);
        Vector3 nextPosition = this.GetNextPosition();
        Debug.DrawLine(transform.position, nextPosition, Color.black, 2f);

        if (Physics.Linecast(transform.position, nextPosition, out RaycastHit hit, LayerMask.GetMask(layerName)))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red, 2f);
            collidePosition = hit.point;
            hit.collider.TryGetComponent(out collideObject);
            return true;
        }

        return false;
    }

    private async void OnWillCollide(Vector3 collidePosition, IDamageable damageable)
    {
        // Set transform to collision point then wait a frame before taking action
        transform.position = collidePosition;
        await UnityTimer.Delay(0);
        damageable.TakeLocalDamage(SoldierDamageController.DamageType.Bullet, this._damageAmount, collidePosition, this._wasShotByLocalPlayer);
        ObjectPoolSystem.Instance.ReleaseObject(ObjectPoolSystem.PoolType.Bullet, transform);
    }
}
