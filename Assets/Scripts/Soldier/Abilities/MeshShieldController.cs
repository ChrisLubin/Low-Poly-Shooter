using UnityEngine;

public class MeshShieldController : MonoBehaviour, IDamageable
{
    void IDamageable.TakeLocalDamage(SoldierDamageController.DamageType _, int __, Vector3 ___, bool ____)
    {
    }
}
