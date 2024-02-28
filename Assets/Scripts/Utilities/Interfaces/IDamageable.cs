using UnityEngine;

public interface IDamageable
{
    public void TakeLocalDamage(DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer);
}
