using Cinemachine;
using UnityEngine;

public class MeshShieldAbilityController : AbilityController
{
    private SoldierCameraController _cameraController;
    private WeaponController _weaponController;

    [SerializeField] private Transform _shield;

    private void Awake()
    {
        this.Ability = Abilities.MeshShield;

        this._cameraController = GetComponent<SoldierCameraController>();
        this._weaponController = GetComponentInChildren<WeaponController>();
    }

    public override void Activate()
    {
        base.Activate();

        if (this._cameraController.IsLocalPlayer)
        {
            CinemachineController.SetBlendDuration(0.5f);
            this._cameraController.EnableThirdPersonCamera();

        }

        this._shield.gameObject.SetActive(true);
        this._weaponController.gameObject.SetActive(false);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        if (this._cameraController.IsLocalPlayer)
        {
            CinemachineController.SetBlendDuration(0.5f);
            this._cameraController.EnableFirstPersonCamera();
        }

        this._shield.gameObject.SetActive(false);
        this._weaponController.gameObject.SetActive(true);
    }
}
