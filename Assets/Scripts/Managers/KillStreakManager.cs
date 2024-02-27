using UnityEngine;

public class KillStreakManager : StaticInstance<KillStreakManager>
{
    [SerializeField] private Transform _predatorMissileSpawnTransform;

    public Vector3 GetPredatorMissileSpawnPoint() => this._predatorMissileSpawnTransform.position;
}
