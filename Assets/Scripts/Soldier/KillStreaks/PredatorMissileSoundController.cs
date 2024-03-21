using UnityEngine;

public class PredatorMissileSoundController : MonoBehaviour
{
    private PredatorMissileMovementController _movementController;

    [SerializeField] private AudioClip _predatorMissileImpactSoundEffect;
    [SerializeField] private AudioClip _predatorMissileDestructionSoundEffect;
    private const float _PREDATOR_MISSILE_EXPLOSION_VOLUME = 0.3f;
    private const float _START_DESTRUCTION_AUDIO_FADE_AT = 0.8f;

    private void Awake()
    {
        this._movementController = GetComponent<PredatorMissileMovementController>();
        this._movementController.OnExploded += this.OnExploded;
    }

    private void OnDestroy()
    {
        this._movementController.OnExploded -= this.OnExploded;
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
