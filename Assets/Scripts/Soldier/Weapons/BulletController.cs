using UnityEngine;

public class BulletController : MonoBehaviour
{
    private float _speed = 0f;
    private float _timeSinceCreation = 0f;
    private float _maxLifeSpan = 2f;

    public void Init(float bulletSpeed) => this._speed = bulletSpeed;

    void Update()
    {
        this._timeSinceCreation += Time.deltaTime;
        transform.position += transform.forward * this._speed * Time.deltaTime;

        if (this._timeSinceCreation > this._maxLifeSpan)
        {
            Destroy(gameObject);
        }
    }
}
