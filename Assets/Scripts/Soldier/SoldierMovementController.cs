using UnityEngine;

public class SoldierMovementController : NetworkBehaviorAutoDisable<SoldierMovementController>
{
    private CharacterController _characterController;
    private WeaponController _weaponController;

    [Header("Base setup")]
    [SerializeField] private float _adsSpeed = 1.8f;
    [SerializeField] private float _walkingSpeed = 4.5f;
    [SerializeField] private float _runningSpeed = 6.5f;
    [SerializeField] private float _jumpSpeed = 8.0f;
    [SerializeField] private float _gravity = 20.0f;
    private Vector3 _moveDirection = Vector3.zero;

    private void Awake()
    {
        this._characterController = GetComponent<CharacterController>();
        this._weaponController = GetComponentInChildren<WeaponController>();
    }

    private void Update()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float movementSpeed = this._weaponController.IsADS ? this._adsSpeed : isRunning ? this._runningSpeed : this._walkingSpeed;
        float curSpeedX = !PauseMenuController.IsPaused ? movementSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = !PauseMenuController.IsPaused ? movementSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = this._moveDirection.y;
        this._moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && this._characterController.isGrounded)
            this._moveDirection.y = this._jumpSpeed;
        else
            this._moveDirection.y = movementDirectionY;

        if (!this._characterController.isGrounded)
            this._moveDirection.y -= this._gravity * Time.deltaTime;

        // Move the controller
        this._characterController.Move(this._moveDirection * Time.deltaTime);
    }
}
