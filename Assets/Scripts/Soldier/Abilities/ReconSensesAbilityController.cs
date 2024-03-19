using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class ReconSensesAbilityController : AbilityController
{
    [SerializeField] private RenderObjects _enemySilhoutteRenderer;

    private void Awake()
    {
        this.Ability = Abilities.ReconSenses;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsOwner) { return; }
        this._enemySilhoutteRenderer.SetActive(false);
    }

    public override void Activate()
    {
        if (!this.IsOwner) { return; }

        base.Activate();

        this.RenderEnemySilhoutte(true);
    }

    public override void Deactivate()
    {
        if (!this.IsOwner) { return; }

        base.Deactivate();

        this.RenderEnemySilhoutte(false);
    }

    private void RenderEnemySilhoutte(bool isReconSensing) => this._enemySilhoutteRenderer.SetActive(isReconSensing);
}
