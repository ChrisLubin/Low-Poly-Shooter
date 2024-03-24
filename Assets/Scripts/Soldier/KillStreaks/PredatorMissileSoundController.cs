using Unity.Netcode;
using UnityEngine;

public class PredatorMissileSoundController : NetworkBehaviour
{
    private PredatorMissileMovementController _movementController;

    [Header("Air")]
    [SerializeField] private AudioSource _airSoundEffectAudioSource;
    [SerializeField] private AudioClip _operatorAirSoundEffect;
    private const float _PREDATOR_MISSILE_OPERATOR_AIR_VOLUME = 0.09f;

    [Header("Impact")]
    [SerializeField] private AudioClip _predatorMissileImpactSoundEffect;
    [SerializeField] private AudioClip _predatorMissileDestructionSoundEffect;
    private const float _PREDATOR_MISSILE_EXPLOSION_VOLUME = 0.3f;
    private const float _START_DESTRUCTION_AUDIO_FADE_AT = 0.8f;

    private void Awake()
    {
        this._movementController = GetComponent<PredatorMissileMovementController>();
        this._movementController.OnExploded += this.OnExploded;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._movementController.OnExploded -= this.OnExploded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (this.IsOwner)
        {
            this._airSoundEffectAudioSource.clip = this._operatorAirSoundEffect;
            this._airSoundEffectAudioSource.volume = _PREDATOR_MISSILE_OPERATOR_AIR_VOLUME;
            this._airSoundEffectAudioSource.Play();
        }
    }

    public void OnExploded(Vector3 explosionPoint)
    {
        Helpers.PlayClipAtPoint(this._predatorMissileImpactSoundEffect, explosionPoint, _PREDATOR_MISSILE_EXPLOSION_VOLUME, out AudioSource impactAudioSource);
        Helpers.PlayClipAtPoint(this._predatorMissileDestructionSoundEffect, explosionPoint, _PREDATOR_MISSILE_EXPLOSION_VOLUME, out AudioSource destructionAudioSource);
        impactAudioSource.rolloffMode = AudioRolloffMode.Linear;
        destructionAudioSource.rolloffMode = AudioRolloffMode.Linear;

        FadeOutOneShotAudioController fadeOutController = (FadeOutOneShotAudioController)destructionAudioSource.gameObject.AddComponent(typeof(FadeOutOneShotAudioController));
        fadeOutController.Init(_START_DESTRUCTION_AUDIO_FADE_AT);
    }
}
