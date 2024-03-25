using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class ReconSensesAbilityController : AbilityController
{
    [SerializeField] private RenderObjects _enemyHiddenRenderer;
    [SerializeField] private Material _silhoutteMaterial;

    private void Awake()
    {
        this.Ability = Abilities.ReconSenses;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsOwner) { return; }
        this._enemyHiddenRenderer.SetActive(false);
    }

    public override void Activate()
    {
        base.Activate();

        if (!this.IsOwner) { return; }
        this.RenderEnemySilhoutte(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        if (!this.IsOwner) { return; }
        this.RenderEnemySilhoutte(false);
    }

    private void RenderEnemySilhoutte(bool isReconSensing)
    {
        if (!isReconSensing && this._enemyHiddenRenderer.isActive && this._enemyHiddenRenderer.settings.overrideMaterial)
        {
            this._enemyHiddenRenderer.settings.overrideMaterial = default;
            this._enemyHiddenRenderer.SetActive(false);
            this._enemyHiddenRenderer.Create();
        }
        else if (isReconSensing)
        {
            this._enemyHiddenRenderer.settings.overrideMaterial = this._silhoutteMaterial;
            this._enemyHiddenRenderer.SetActive(true);
            this._enemyHiddenRenderer.Create();
        }
    }
}
