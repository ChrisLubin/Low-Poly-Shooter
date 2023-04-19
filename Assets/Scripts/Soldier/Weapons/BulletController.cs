using UnityEngine;

public class BulletController : MonoBehaviour
{
    private Vector3 _targetDirection;
    private float _speed;
    private float _timeSinceCreation = 0f;
    private float _maxLifeSpan = 2f;

    public void Setup(Vector3 directionToGo, float bulletSpeed)
    {
        this._speed = bulletSpeed;
        transform.LookAt(directionToGo);
        this._targetDirection = (directionToGo - transform.position).normalized;
    }

    void Update()
    {
        this._timeSinceCreation += Time.deltaTime;
        transform.position += this._targetDirection * this._speed * Time.deltaTime;

        if (this._timeSinceCreation > this._maxLifeSpan)
        {
            Destroy(gameObject);
        }
    }
}
