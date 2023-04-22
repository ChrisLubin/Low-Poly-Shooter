using System.Threading.Tasks;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private float _speed = 0f;
    private float _timeSinceCreation = 0f;
    private float _maxLifeSpan = 2f;

    public void Init(float bulletSpeed) => this._speed = bulletSpeed;

    private void Start()
    {
        if (this.WillCollideNextFrame(out Vector3 collidePosition, out SoldierController soldier, Constants.LayerNames.Soldier))
        {
            this.OnSoldierWillCollide(collidePosition, soldier);
        }
    }

    private void FixedUpdate()
    {
        if (this.WillCollideNextFrame(out Vector3 collidePosition, out SoldierController soldier, Constants.LayerNames.Soldier))
        {
            this.OnSoldierWillCollide(collidePosition, soldier);
            return;
        }
        this._timeSinceCreation += Time.fixedDeltaTime;
        transform.position = this.GetNextPosition();

        if (this._timeSinceCreation > this._maxLifeSpan)
        {
            Destroy(gameObject);
        }
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

    private async void OnSoldierWillCollide(Vector3 collidePosition, SoldierController soldier)
    {
        // Set transform to collision point then wait a frame before taking action
        transform.position = collidePosition;
        await Task.Delay(0);
        soldier.TakeLocalDamage(SoldierDamageController.DamageType.Bullet);
        Destroy(gameObject);
    }
}
