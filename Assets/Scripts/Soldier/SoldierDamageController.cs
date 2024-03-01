using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierDamageController : NetworkBehaviour, IDamageable
{
    private SoldierHealthController _healthController;
    [SerializeField] private Transform _bloodSplatterVfxPrefab;
    [SerializeField] private AudioClip _bulletFleshImpactAudioClip;

    private const float _BULLET_IMPACT_AUDIO_VOLUME = 0.3f;

    public event Action<Vector3, DamageType, int> OnPlayerDamagedByLocalPlayer;
    public event Action<ulong, Vector3, DamageType, int> OnServerTakeDamage;
    public event Action<DamageType, int> OnServerDamageReceived;

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
            AudioSource.PlayClipAtPoint(this._bulletFleshImpactAudioClip, damagePoint, _BULLET_IMPACT_AUDIO_VOLUME);

        this.OnPlayerDamagedByLocalPlayer?.Invoke(damagePoint, type, damageAmount);
    }

    public void TakeServerDamage(ulong damagerClientId, Vector3 damagePoint, DamageType type, int damageAmount)
    {
        if (!this.IsHost) { return; }
        // Host sending damage to player

        this.OnServerTakeDamage?.Invoke(damagerClientId, damagePoint, type, damageAmount);
    }

    private void OnHealthChange(HealthData oldHealthData, HealthData newHealthData)
    {
        if (newHealthData.Health >= oldHealthData.Health) { return; }
        // Clients reacting to host sending damage to player

        if (newHealthData.LatestDamageType == DamageType.Bullet && newHealthData.LatestDamagerClientId != NetworkManager.Singleton.LocalClientId)
            AudioSource.PlayClipAtPoint(this._bulletFleshImpactAudioClip, transform.position, _BULLET_IMPACT_AUDIO_VOLUME);

        this.OnServerDamageReceived?.Invoke(newHealthData.LatestDamageType, oldHealthData.Health - newHealthData.Health);
    }
}

[Serializable]
public enum DamageType
{
    None,
    Bullet,
    Grenade,
    Missile
}
