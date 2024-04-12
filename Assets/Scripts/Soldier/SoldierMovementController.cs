using InfimaGames.Animated.ModernGuns;
using UnityEngine;

public class SoldierMovementController : NetworkBehaviorAutoDisable<SoldierMovementController>
{
    private Character _character;
    private CharacterController _characterController;
    // private WeaponController _weaponController;

    [Header("Base setup")]
    [SerializeField] private float _adsSpeed = 1.4f;
    [SerializeField] private float _walkingSpeed = 3f;
    [SerializeField] private float _runningSpeed = 6f;
    [SerializeField] private float _jumpSpeed = 8f;
    [SerializeField] private float _gravity = 20f;
    private Vector3 _moveDirection = Vector3.zero;

    private void Awake()
    {
        this._character = GetComponent<Character>();
        this._characterController = GetComponent<CharacterController>();
        // this._weaponController = GetComponentInChildren<WeaponController>();
    }

    private void Update()
    {
        bool isRunning = this._character.IsRunning;

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float movementSpeed = this._character.IsAiming ? this._adsSpeed : isRunning ? this._runningSpeed : this._walkingSpeed;
        float curSpeedX = PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || SoldierKillStreakController.IS_USING_KILL_STREAK ? 0 : movementSpeed * Input.GetAxis("Vertical");
        float curSpeedY = PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || SoldierKillStreakController.IS_USING_KILL_STREAK ? 0 : movementSpeed * Input.GetAxis("Horizontal");
        float movementDirectionY = this._moveDirection.y;
        this._moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (!PauseMenuController.IsPaused && !SoldierKillStreakController.IS_USING_KILL_STREAK && Input.GetButtonDown("Jump") && this._characterController.isGrounded)
            this._moveDirection.y = this._jumpSpeed;
        else
            this._moveDirection.y = movementDirectionY;

        if (!this._characterController.isGrounded)
            this._moveDirection.y -= this._gravity * Time.deltaTime;

        // Move the controller
        this._characterController.Move(this._moveDirection * Time.deltaTime);
    }
}
