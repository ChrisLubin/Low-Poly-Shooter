using UnityEngine;

public class SoldierCameraController : NetworkBehaviorAutoDisable<SoldierCameraController>
{
    [SerializeField] private Camera _firstPersonCamera;
    [SerializeField] private Camera _thirdPersonCamera;
    [SerializeField] private float _lookSpeed = 2.0f;
    [SerializeField] private float _lookXLimit = 45.0f;

    private float _rotationX = 0;
    public new bool IsLocalPlayer => this.IsOwner;

    protected override void OnOwnerNetworkSpawn()
    {
        this._firstPersonCamera.enabled = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (PauseMenuController.IsPaused) { return; }

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
}
