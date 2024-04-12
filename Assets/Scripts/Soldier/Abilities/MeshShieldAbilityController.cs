using Cinemachine;
using UnityEngine;

public class MeshShieldAbilityController : AbilityController
{
    private SoldierCameraController _cameraController;

    [SerializeField] private Transform _shield;
    [SerializeField] private GameObject[] _disableOnActive;

    private void Awake()
    {
        this.Ability = Abilities.MeshShield;

        this._cameraController = GetComponent<SoldierCameraController>();
    }

    public override void Activate()
    {
        base.Activate();

        this._shield.gameObject.SetActive(true);
        foreach (GameObject gameObjToDisable in this._disableOnActive)
            gameObjToDisable.SetActive(false);

        if (!this._cameraController.IsLocalPlayer) { return; }

        if (!SoldierKillStreakController.IS_USING_KILL_STREAK)
            CinemachineController.SetBlendDuration(0.5f);
        this._cameraController.EnableThirdPersonCamera();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        this._shield.gameObject.SetActive(false);
        foreach (GameObject gameObjToEnable in this._disableOnActive)
            gameObjToEnable.SetActive(true);

        if (!this._cameraController.IsLocalPlayer) { return; }

        if (!SoldierKillStreakController.IS_USING_KILL_STREAK)
            CinemachineController.SetBlendDuration(0.5f);
        this._cameraController.EnableFirstPersonCamera();
    }
}
