using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    [Header("Predator Missile")]
    [SerializeField] private AudioClip _predatorMissileAttainedSoundEffect;
    private const float _PREDATOR_MISSILE_ATTAINED_AUDIO_VOLUME = 0.05f;

    [SerializeField] private AudioClip _enemyPredatorMissileIncomingVoiceSoundEffect;
    private const float _ENEMY_PREDATOR_MISSILE_INCOMING_VOICE_AUDIO_VOLUME = 0.05f;

    private void Awake()
    {
        SoldierKillStreakController.OnLocalPlayerKillStreakAttained += this.OnLocalPlayerKillStreakAttained;
        SoldierKillStreakController.OnNonLocalPlayerKillStreakActivated += this.OnNonLocalPlayerKillStreakActivated;
    }

    private void OnDestroy()
    {
        SoldierKillStreakController.OnLocalPlayerKillStreakAttained -= this.OnLocalPlayerKillStreakAttained;
        SoldierKillStreakController.OnNonLocalPlayerKillStreakActivated -= this.OnNonLocalPlayerKillStreakActivated;
    }

    private void OnLocalPlayerKillStreakAttained()
    {
        this._audioSource.volume = _PREDATOR_MISSILE_ATTAINED_AUDIO_VOLUME;
        this._audioSource.clip = this._predatorMissileAttainedSoundEffect;
        this._audioSource.Play();
    }

    private void OnNonLocalPlayerKillStreakActivated()
    {
        this._audioSource.volume = _ENEMY_PREDATOR_MISSILE_INCOMING_VOICE_AUDIO_VOLUME;
        this._audioSource.clip = this._enemyPredatorMissileIncomingVoiceSoundEffect;
        this._audioSource.Play();
    }
}
