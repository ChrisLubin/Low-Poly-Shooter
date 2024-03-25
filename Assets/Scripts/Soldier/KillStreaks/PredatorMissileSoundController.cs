using Unity.Netcode;
using UnityEngine;

public class PredatorMissileSoundController : NetworkBehaviour
{
    private PredatorMissileMovementController _movementController;

    [Header("Air")]
    [SerializeField] private AudioSource _airSoundEffectAudioSource;
    [SerializeField] private AudioClip _airSoundEffect;
    [SerializeField] private AnimationCurve _nonOperatorAirSoundCurve;
    private const float _PREDATOR_MISSILE_OPERATOR_AIR_VOLUME = 0.09f;
    private float _spawnPositionY;

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

    private void Start()
    {
        this._spawnPositionY = transform.position.y;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._movementController.OnExploded -= this.OnExploded;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        this._airSoundEffectAudioSource.clip = this._airSoundEffect;
        this._airSoundEffectAudioSource.volume = _PREDATOR_MISSILE_OPERATOR_AIR_VOLUME;
        this._airSoundEffectAudioSource.Play();

        if (!this.IsOwner)
        {
            this._airSoundEffectAudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, this._nonOperatorAirSoundCurve);
            this._airSoundEffectAudioSource.rolloffMode = AudioRolloffMode.Custom;
        }
    }

    private void Update()
    {
        if (this.IsOwner) { return; }

        // Change pitch as missile gets closer to ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            float distanceToGround = Mathf.Abs(hit.point.y - transform.position.y);
            this._airSoundEffectAudioSource.pitch = Mathf.Lerp(1f, 3f, Mathf.InverseLerp(this._spawnPositionY, 0f, distanceToGround));
        }
    }

    public void OnExploded(Vector3 explosionPoint)
    {
        Helpers.PlayClipAtPoint(this._predatorMissileImpactSoundEffect, explosionPoint, _PREDATOR_MISSILE_EXPLOSION_VOLUME, out AudioSource impactAudioSource);
        Helpers.PlayClipAtPoint(this._predatorMissileDestructionSoundEffect, explosionPoint, _PREDATOR_MISSILE_EXPLOSION_VOLUME, out AudioSource destructionAudioSource);
        impactAudioSource.rolloffMode = AudioRolloffMode.Linear;
        destructionAudioSource.rolloffMode = AudioRolloffMode.Linear;

        FadeOutOneShotAudioController fadeOutController = destructionAudioSource.gameObject.AddComponent<FadeOutOneShotAudioController>();
        fadeOutController.Init(_START_DESTRUCTION_AUDIO_FADE_AT);
    }
}
