using Unity.Netcode;
using UnityEngine;

public class SoldierMovementController : NetworkBehaviour
{
    [Header("Base setup")]
    [SerializeField] private float _adsSpeed = 1.8f;
    [SerializeField] private float _walkingSpeed = 4.5f;
    [SerializeField] private float _runningSpeed = 6.5f;
    [SerializeField] private float _jumpSpeed = 8.0f;
    [SerializeField] private float _gravity = 20.0f;
    [SerializeField] private float _lookSpeed = 2.0f;
    [SerializeField] private float _lookXLimit = 45.0f;
    private CharacterController _characterController;
    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;
    private bool _canMove = true;
    [SerializeField] private float _cameraYOffset = 1.7f;
    [SerializeField] private Camera _playerCamera;
    public bool IsGrounded => this._characterController.isGrounded;

    private WeaponAnimationController _weaponAnimationController;
    private bool _isADS = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsOwner)
        {
            this.enabled = false;
            return;
        }

        this._characterController = GetComponent<CharacterController>();
        this._weaponAnimationController = GetComponentInChildren<WeaponAnimationController>();

        this._weaponAnimationController.OnADSEvent += this.OnADSEvent;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._weaponAnimationController.OnADSEvent -= this.OnADSEvent;
    }

    private void OnADSEvent(bool isADS) => this._isADS = isADS;

    private void Update()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float movementSpeed = this._isADS ? this._adsSpeed : isRunning ? this._runningSpeed : this._walkingSpeed;
        float curSpeedX = this._canMove ? movementSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = this._canMove ? movementSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = this._moveDirection.y;
        this._moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && this._canMove && this._characterController.isGrounded)
        {
            this._moveDirection.y = this._jumpSpeed;
        }
        else
        {
            this._moveDirection.y = movementDirectionY;
        }

        if (!this._characterController.isGrounded)
        {
            this._moveDirection.y -= this._gravity * Time.deltaTime;
        }

        // Move the controller
        this._characterController.Move(this._moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (this._canMove && this._playerCamera != null)
        {
            this._rotationX += -Input.GetAxis("Mouse Y") * this._lookSpeed;
            this._rotationX = Mathf.Clamp(this._rotationX, -this._lookXLimit, this._lookXLimit);
            this._playerCamera.transform.localRotation = Quaternion.Euler(this._rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * this._lookSpeed, 0);
        }
    }
}
