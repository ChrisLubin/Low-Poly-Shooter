using UnityEngine;

public class MuzzleFlashController : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float _maxLifeSpan = 0.2f;

    private void Awake() => this._particleSystem = GetComponent<ParticleSystem>();

    public async void Init()
    {
        this._particleSystem.Play(true);
        await UnityTimer.Delay(this._maxLifeSpan);
        this._particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        ObjectPoolSystem.Instance.ReleaseObject(ObjectPoolSystem.PoolType.MuzzleFlash, transform);
    }
}
