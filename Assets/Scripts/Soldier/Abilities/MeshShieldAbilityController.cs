using UnityEngine;

public class MeshShieldAbilityController : AbilityController
{
    private SoldierCameraController _cameraController;

    [SerializeField] private Transform _shield;

    private void Awake()
    {
        this.Ability = Abilities.MeshShield;

        this._cameraController = GetComponent<SoldierCameraController>();
    }

    public override void Activate()
    {
        base.Activate();

        if (this._cameraController.IsLocalPlayer)
            this._cameraController.EnableThirdPersonCamera();

        this._shield.gameObject.SetActive(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        if (this._cameraController.IsLocalPlayer)
            this._cameraController.EnableFirstPersonCamera();

        this._shield.gameObject.SetActive(false);
    }
}
