using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    [SerializeField] private AudioSource _generalSource;
    [SerializeField] private AudioSource _hitMarkerSource;

    [Header("Predator Missile")]
    [SerializeField] private AudioClip _predatorMissileAttainedSoundEffect;
    private const float _PREDATOR_MISSILE_ATTAINED_AUDIO_VOLUME = 0.05f;

    [SerializeField] private AudioClip _enemyPredatorMissileIncomingVoiceSoundEffect;
    private const float _ENEMY_PREDATOR_MISSILE_INCOMING_VOICE_AUDIO_VOLUME = 0.05f;

    [Header("Hit Marker")]
    [SerializeField] private AudioClip _hitMarkerSoundEffect;
    private const float _HIT_MARKER_AUDIO_VOUME = 0.3f;

    private void Awake()
    {
        SoldierManager.OnPlayerDamagedByLocalPlayer += this.OnPlayerDamagedByLocalPlayer;
        SoldierKillStreakController.OnLocalPlayerKillStreakAttained += this.OnLocalPlayerKillStreakAttained;
        SoldierKillStreakController.OnNonLocalPlayerKillStreakActivated += this.OnNonLocalPlayerKillStreakActivated;
    }

    private void OnDestroy()
    {
        SoldierManager.OnPlayerDamagedByLocalPlayer -= this.OnPlayerDamagedByLocalPlayer;
        SoldierKillStreakController.OnLocalPlayerKillStreakAttained -= this.OnLocalPlayerKillStreakAttained;
        SoldierKillStreakController.OnNonLocalPlayerKillStreakActivated -= this.OnNonLocalPlayerKillStreakActivated;
    }

    private void OnLocalPlayerKillStreakAttained()
    {
        this._generalSource.volume = _PREDATOR_MISSILE_ATTAINED_AUDIO_VOLUME;
        this._generalSource.clip = this._predatorMissileAttainedSoundEffect;
        this._generalSource.Play();
    }

    private void OnNonLocalPlayerKillStreakActivated()
    {
        this._generalSource.volume = _ENEMY_PREDATOR_MISSILE_INCOMING_VOICE_AUDIO_VOLUME;
        this._generalSource.clip = this._enemyPredatorMissileIncomingVoiceSoundEffect;
        this._generalSource.Play();
    }

    private void OnPlayerDamagedByLocalPlayer(DamageType damageType)
    {
        if (damageType != DamageType.Bullet) { return; }

        this._hitMarkerSource.volume = _HIT_MARKER_AUDIO_VOUME;
        this._hitMarkerSource.clip = this._hitMarkerSoundEffect;
        this._hitMarkerSource.Play();
    }
}
