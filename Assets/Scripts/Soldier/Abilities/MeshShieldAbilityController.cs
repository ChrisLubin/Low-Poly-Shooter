using UnityEngine;

public class MeshShieldAbilityController : AbilityController
{
    [SerializeField] private Transform _shield;

    private void Awake()
    {
        this.Ability = Abilities.MeshShield;
    }

    public override void Activate()
    {
        base.Activate();
        this._shield.gameObject.SetActive(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        this._shield.gameObject.SetActive(false);
    }
}
