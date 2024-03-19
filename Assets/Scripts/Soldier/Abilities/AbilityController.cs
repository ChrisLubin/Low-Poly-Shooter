using System;
using Unity.Netcode;
using UnityEngine;

public abstract class AbilityController : NetworkBehaviour
{
    // Properties needed to be set by ability
    public Abilities Ability { get; protected set; }

    public bool IsActive { get; protected set; }
    public event Action OnInternallyDeactivated;

    [SerializeField] private AudioClip _abilityToggledAudioClip;
    [SerializeField] private float _toggleAbilityAudioVolume = 0.5f;

    public virtual bool CanActivate() => !this.IsActive;
    public virtual void Activate()
    {
        if (!this.IsActive && this._abilityToggledAudioClip != null)
            AudioSource.PlayClipAtPoint(this._abilityToggledAudioClip, transform.position, this._toggleAbilityAudioVolume);

        this.IsActive = true;
    }
    public virtual void Deactivate()
    {
        if (this.IsActive && this._abilityToggledAudioClip != null)
            AudioSource.PlayClipAtPoint(this._abilityToggledAudioClip, transform.position, this._toggleAbilityAudioVolume);

        this.IsActive = false;
    }

    protected void DeactivateInternally()
    {
        this.Deactivate();
        this.OnInternallyDeactivated?.Invoke();
    }
}

public enum Abilities
{
    None,
    Invisibility,
    MeshShield,
    ReconSenses,
}
