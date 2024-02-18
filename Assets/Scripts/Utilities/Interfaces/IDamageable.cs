using UnityEngine;

public interface IDamageable
{
    public void TakeLocalDamage(SoldierDamageController.DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer);
}
