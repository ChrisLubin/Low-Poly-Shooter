using UnityEngine;
using static SoldierDamageController;

public class MeshShieldController : MonoBehaviour, IDamageable
{
    [SerializeField] private AudioClip _bulletImpactAudioClip;

    private const float _BULLET_IMPACT_AUDIO_VOLUME = 0.1f;

    void IDamageable.TakeLocalDamage(DamageType type, int _, Vector3 damagePoint, bool __)
    {
        if (type == DamageType.Bullet)
            AudioSource.PlayClipAtPoint(this._bulletImpactAudioClip, damagePoint, _BULLET_IMPACT_AUDIO_VOLUME);
    }
}
