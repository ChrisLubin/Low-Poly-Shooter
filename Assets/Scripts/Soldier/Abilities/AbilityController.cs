using System;
using UnityEngine;

public abstract class AbilityController : MonoBehaviour
{
    // Properties needed to be set by ability
    public Abilities Ability { get; protected set; }

    public bool IsActive { get; protected set; }
    public event Action OnDeactivate;

    public virtual bool CanActivate() => !this.IsActive;
    public virtual void Activate() => this.IsActive = true;

    public virtual void Deactivate()
    {
        this.IsActive = false;
        this.OnDeactivate?.Invoke();
    }
}

public enum Abilities
{
    None,
    Invisibility,
}
