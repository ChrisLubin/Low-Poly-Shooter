using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierDamageController : NetworkBehaviour, IDamageable
{
    private SoldierHealthController _healthController;
    [SerializeField] private Transform _bloodSplatterVfxPrefab;
    [SerializeField] private AudioClip _bulletFleshImpactAudioClip;

    private const float _BULLET_IMPACT_AUDIO_VOLUME = 0.3f;

    public event Action<DamageType, int> OnNonLocalPlayerShotByLocalPlayer;
    public event Action<ulong, DamageType, int> OnServerTakeDamage;
    public event Action<DamageType, int> OnServerDamageReceived;

    [Serializable]
    public enum DamageType
    {
        Bullet,
        Grenade,
        Missile
    }

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

    public void TakeLocalDamage(DamageType type, int damageAmount, Vector3 damagePoint, bool isDamageFromLocalPlayer)
    {
        if (!this.IsOwner)
            Instantiate(this._bloodSplatterVfxPrefab, damagePoint, Quaternion.identity, transform);

        // Only take damage when another client says we have
        if (!isDamageFromLocalPlayer) { return; }

        // We shot another soldier locally
        if (type == DamageType.Bullet)
        {
            AudioSource.PlayClipAtPoint(this._bulletFleshImpactAudioClip, damagePoint, _BULLET_IMPACT_AUDIO_VOLUME);
            this.OnNonLocalPlayerShotByLocalPlayer?.Invoke(type, damageAmount);
        }
    }

    public void TakeServerDamage(ulong damagerClientId, DamageType type, int damageAmount)
    {
        if (!this.IsHost) { return; }
        // Host sending damage to player

        if (type == DamageType.Bullet)
            this.OnServerTakeDamage?.Invoke(damagerClientId, type, damageAmount);
    }

    private void OnHealthChange(HealthData oldHealthData, HealthData newHealthData)
    {
        if (newHealthData.Health >= oldHealthData.Health) { return; }
        // Clients reacting to host sending damage to player

        if (newHealthData.LatestDamagerClientId != NetworkManager.Singleton.LocalClientId)
            AudioSource.PlayClipAtPoint(this._bulletFleshImpactAudioClip, transform.position, _BULLET_IMPACT_AUDIO_VOLUME);

        this.OnServerDamageReceived?.Invoke(newHealthData.LatestDamageType, oldHealthData.Health - newHealthData.Health);
    }
}
