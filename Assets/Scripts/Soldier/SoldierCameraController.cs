// using Cinemachine;
using UnityEngine;

public class SoldierCameraController : NetworkBehaviorAutoDisable<SoldierCameraController>
{
    [SerializeField] private Transform _neck;
    // [SerializeField] private CinemachineVirtualCamera _firstPersonCamera;
    // [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;
    [SerializeField] private float _lookSpeed = 40f;
    [SerializeField] private float _lookXLimit = 55f;

    private float _neckCenterRotationX;
    private float _rotationX = 0f;
    public new bool IsLocalPlayer => this.IsOwner;

    public const float SOLDIER_SPAWN_CAMERA_TRANSITION_TIME = 2f;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
    }

    private void Start()
    {
        this._neckCenterRotationX = this._neck.localRotation.x;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.OnStateChange -= this.OnGameStateChange;
    }

    protected override void OnOwnerNetworkSpawn()
    {
        CinemachineController.SetBlendDuration(SOLDIER_SPAWN_CAMERA_TRANSITION_TIME);
        // this._firstPersonCamera.enabled = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || SoldierKillStreakController.IS_USING_KILL_STREAK) { return; }

        this._rotationX += -Input.GetAxis("Mouse Y") * this._lookSpeed * Time.deltaTime;
        this._rotationX = Mathf.Clamp(this._rotationX, this._neckCenterRotationX - this._lookXLimit, this._neckCenterRotationX + this._lookXLimit);
        this._neck.localRotation = Quaternion.Euler(this._rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * this._lookSpeed * Time.deltaTime, 0);
    }

    public void EnableFirstPersonCamera()
    {
        // this._firstPersonCamera.enabled = true;
        // this._thirdPersonCamera.enabled = false;
    }

    public void EnableThirdPersonCamera()
    {
        // this._firstPersonCamera.enabled = false;
        // this._thirdPersonCamera.enabled = true;
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                CinemachineController.SetBlendDuration(4f);
                // this._firstPersonCamera.enabled = false;
                // this._thirdPersonCamera.enabled = false;
                break;
            default:
                break;
        }
    }
}
