using Cinemachine;
using UnityEngine;

public class SoldierCameraController : NetworkBehaviorAutoDisable<SoldierCameraController>
{
    [SerializeField] private CinemachineVirtualCamera _firstPersonCamera;
    [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;
    [SerializeField] private float _lookSpeed = 2.0f;
    [SerializeField] private float _lookXLimit = 45.0f;

    private float _rotationX = 0;
    public new bool IsLocalPlayer => this.IsOwner;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.OnStateChange -= this.OnGameStateChange;
    }

    protected override void OnOwnerNetworkSpawn()
    {
        CinemachineController.SetBlendDuration(2f);
        this._firstPersonCamera.enabled = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (PauseMenuController.IsPaused || GameManager.State == GameState.GameOver) { return; }

        this._rotationX += -Input.GetAxis("Mouse Y") * this._lookSpeed;
        this._rotationX = Mathf.Clamp(this._rotationX, -this._lookXLimit, this._lookXLimit);
        this._firstPersonCamera.transform.localRotation = Quaternion.Euler(this._rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * this._lookSpeed, 0);
    }

    public void EnableFirstPersonCamera()
    {
        this._firstPersonCamera.enabled = true;
        this._thirdPersonCamera.enabled = false;
    }

    public void EnableThirdPersonCamera()
    {
        this._firstPersonCamera.enabled = false;
        this._thirdPersonCamera.enabled = true;
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                this._firstPersonCamera.enabled = false;
                this._thirdPersonCamera.enabled = false;
                break;
            default:
                break;
        }
    }
}
