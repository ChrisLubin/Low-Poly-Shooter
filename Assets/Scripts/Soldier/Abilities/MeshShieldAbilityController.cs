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

        this._shield.gameObject.SetActive(true);
        this._weaponController.gameObject.SetActive(false);

        if (!this._cameraController.IsLocalPlayer) { return; }

        if (!SoldierKillStreakController.IS_USING_KILL_STREAK)
            CinemachineController.SetBlendDuration(0.5f);
        this._cameraController.EnableThirdPersonCamera();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        this._shield.gameObject.SetActive(false);
        this._weaponController.gameObject.SetActive(true);

        if (!this._cameraController.IsLocalPlayer) { return; }

        if (!SoldierKillStreakController.IS_USING_KILL_STREAK)
            CinemachineController.SetBlendDuration(0.5f);
        this._cameraController.EnableFirstPersonCamera();
    }
}
