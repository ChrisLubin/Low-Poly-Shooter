using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierDamageController : NetworkBehaviour
{
    [SerializeField] private Transform _bloodSplatterVfxPrefab;

    public event Action<DamageType, int> OnDamageReceived;

    [Serializable]
    public enum DamageType
    {
        Bullet,
        Grenade,
        Missile
    }

    public void TakeLocalDamage(DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer)
    {
        if (!this.IsOwner)
        {
            Instantiate(this._bloodSplatterVfxPrefab, damagePoint, Quaternion.identity, transform);
        }
        // Only take damage when another client says we have
        if (!isDamageFromLocalPlayer) { return; }

        if (type == DamageType.Bullet)
        {
            // We shot another soldier locally
            this.OnDamageReceived?.Invoke(type, damageAmount);
        }
    }

    public void TakeServerDamage(DamageType type, int damageAmount)
    {
        // Only take damage when another client says we have
        if (!this.IsOwner) { return; }

        if (type == DamageType.Bullet)
        {
            // We got shot by another client
            this.OnDamageReceived?.Invoke(type, damageAmount);
        }
    }
}
