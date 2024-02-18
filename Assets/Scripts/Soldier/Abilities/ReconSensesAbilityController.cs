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
        _enemySilhoutteRenderer.SetActive(false);
    }

    public override void Activate()
    {
        base.Activate();

        RenderEnemySilhoutte(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        RenderEnemySilhoutte(false);
    }

    private void RenderEnemySilhoutte(bool isReconSensing)
    {
        _enemySilhoutteRenderer.SetActive(isReconSensing);
    }
}
