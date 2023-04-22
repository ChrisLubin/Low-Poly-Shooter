using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierDamageController : NetworkBehaviour
{
    [SerializeField] private Transform _bloodSplatterVfxPrefab;

    public Action<DamageType> OnDamageReceived;

    [Serializable]
    public enum DamageType
    {
        Bullet,
        Grenade,
        Missile
    }

    public void TakeLocalDamage(DamageType type, Vector3 damagePoint)
    {
        // Only take damage when another client says we have
        if (this.IsOwner) { return; }

        Instantiate(this._bloodSplatterVfxPrefab, damagePoint, Quaternion.identity, transform);

        if (type == DamageType.Bullet)
        {
            // We shot another soldier locally
            this.OnDamageReceived?.Invoke(type);
        }
    }

    public void TakeServerDamage(DamageType type)
    {
        // Only take damage when another client says we have
        if (!this.IsOwner) { return; }

        if (type == DamageType.Bullet)
        {
            // We got shot by another client
            this.OnDamageReceived?.Invoke(type);
        }
    }
}
