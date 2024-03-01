using System;
using Unity.Netcode;
using UnityEngine;

public class PredatorMissileDamageController : NetworkBehaviour
{
    private PredatorMissileMovementController _movementController;

    [SerializeField] private Transform _explosionPrefab;
    [SerializeField] private float _blastRadius = 15f;

    public static event Action OnLocalPlayerMissileExploded;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, this._blastRadius);
    }

    private void Awake()
    {
        this._movementController = GetComponent<PredatorMissileMovementController>();
        this._movementController.OnExploded += this.OnExploded;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._movementController.OnExploded -= this.OnExploded;
    }

    private void OnExploded(Vector3 explodePosition)
    {
        Instantiate(this._explosionPrefab, explodePosition, Quaternion.identity);

        if (!this.IsOwner) { return; }

        PredatorMissileDamageController.OnLocalPlayerMissileExploded?.Invoke();

        if (Helpers.TrySphereCastAll(explodePosition, this._blastRadius, out CastAllData<SoldierDamageController>[] castDatas, Constants.LayerNames.Soldier))
            foreach (CastAllData<SoldierDamageController> castData in castDatas)
                castData.HitObject.TakeLocalDamage(DamageType.Missile, SoldierHealthController.MAX_HEALTH, explodePosition, true);
    }
}
