using System.Linq;
using QFSW.QC.Editor.Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class ReconSensesAbilityController : AbilityController
{
    [SerializeField] private RenderObjects _enemySilhoutteRenderer;

    private void Awake()
    {
        this.Ability = Abilities.ReconSenses;
    }

    private void Start()
    {
        this._enemySilhoutteRenderer.SetActive(false);
    }

    public override void Activate()
    {
        base.Activate();

        this.RenderEnemySilhoutte(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        this.RenderEnemySilhoutte(false);
    }

    private void RenderEnemySilhoutte(bool isReconSensing) => this._enemySilhoutteRenderer.SetActive(isReconSensing);
}
