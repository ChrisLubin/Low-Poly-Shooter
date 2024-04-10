using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierDeathController : NetworkBehaviour
{
    private SoldierHealthController _healthController;

    [SerializeField] private Transform _soldierRootBone;
    [SerializeField] private Transform _ragdollPrefab;

    public event Action<ulong, DamageType> OnDeath;

    private void Awake()
    {
        this._healthController = GetComponent<SoldierHealthController>();
        this._healthController.OnHealthChange += this.OnHealthChange;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._healthController.OnHealthChange -= this.OnHealthChange;
    }

    private void OnHealthChange(HealthData _, HealthData newHealthData)
    {
        if (newHealthData.Health > SoldierHealthController.MIN_HEALTH) { return; }
        this.gameObject.SetActive(false);

        // Transform ragdollTransform = Instantiate(this._ragdollPrefab, transform.position, transform.rotation);
        // SoldierRagdollController ragdollController = ragdollTransform.GetComponent<SoldierRagdollController>();
        // ragdollController.DoRagroll(this._soldierRootBone, this.IsOwner, newHealthData.LatestDamagePoint, newHealthData.LatestDamageType);
        this.OnDeath?.Invoke(newHealthData.LatestDamagerClientId, newHealthData.LatestDamageType);
    }
}
