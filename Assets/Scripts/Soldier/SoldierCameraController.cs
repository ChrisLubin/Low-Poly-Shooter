using UnityEngine;

public class SoldierCameraController : NetworkBehaviorAutoDisable<SoldierCameraController>
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _lookSpeed = 2.0f;
    [SerializeField] private float _lookXLimit = 45.0f;

    private float _rotationX = 0;

    protected override void OnOwnerNetworkSpawn()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (this._playerCamera == null || PauseMenuController.IsPaused) { return; }

        this._rotationX += -Input.GetAxis("Mouse Y") * this._lookSpeed;
        this._rotationX = Mathf.Clamp(this._rotationX, -this._lookXLimit, this._lookXLimit);
        this._playerCamera.transform.localRotation = Quaternion.Euler(this._rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * this._lookSpeed, 0);
    }
}
