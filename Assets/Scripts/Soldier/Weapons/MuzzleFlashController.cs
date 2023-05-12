using UnityEngine;

public class MuzzleFlashController : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Transform _shootPoint;
    private const int _MAX_LIFE_SPAN = 200;

    private void Awake() => this._particleSystem = GetComponent<ParticleSystem>();

    public async void Init(Transform shootPoint)
    {
        this._shootPoint = shootPoint;
        this._particleSystem.Play(true);
        await UnityTimer.Delay(_MAX_LIFE_SPAN);
        this._particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        ObjectPoolSystem.Instance.ReleaseObject(ObjectPoolSystem.PoolType.MuzzleFlash, transform);
    }

    private void Update()
    {
        if (this._shootPoint == null) { return; }

        transform.position = this._shootPoint.position;
    }
}
