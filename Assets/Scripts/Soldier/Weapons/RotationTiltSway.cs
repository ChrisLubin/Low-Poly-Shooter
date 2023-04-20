using UnityEngine;

public class RotationTiltSway : NetworkBehaviorAutoDisable<RotationTiltSway>
{
    [Header("Position")]
    [SerializeField] private float _amount = 0.02f;
    [SerializeField] private float _maxAmonut = 0.06f;
    [SerializeField] private float _smoothAmount = 6f;

    [Header("Rotation")]
    [SerializeField] private float _rotationAmount = 4f;
    [SerializeField] private float _maxRotationAmount = 5f;
    [SerializeField] private float _smoothRotation = 12f;

    [Header("Axes to rotate")]
    [SerializeField] private bool _shouldRotateX = true;
    [SerializeField] private bool _shouldRotateY = true;
    [SerializeField] private bool _shouldRotateZ = true;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private float _inputX;
    private float _inputY;

    void Start()
    {
        this._initialPosition = transform.localPosition;
        this._initialRotation = transform.localRotation;
    }

    void Update()
    {
        CalculateSway();

        MoveSway();
        TiltSway();
    }

    private void CalculateSway()
    {
        this._inputX = -Input.GetAxis("Mouse X");
        this._inputY = -Input.GetAxis("Mouse Y");
    }

    private void MoveSway()
    {
        float moveX = Mathf.Clamp(this._inputX * this._amount, -this._maxAmonut, this._maxAmonut);
        float moveY = Mathf.Clamp(this._inputY * this._amount, -this._maxAmonut, this._maxAmonut);

        Vector3 finalPosition = new(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + this._initialPosition, Time.deltaTime * this._smoothAmount);
    }

    private void TiltSway()
    {
        float tiltY = Mathf.Clamp(this._inputX * this._rotationAmount, -this._maxRotationAmount, this._maxRotationAmount);
        float tiltX = Mathf.Clamp(this._inputY * this._rotationAmount, -this._maxRotationAmount, this._maxRotationAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(this._shouldRotateX ? -tiltX : 0f, this._shouldRotateY ? tiltY : 0f, this._shouldRotateZ ? tiltY : 0f));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * this._initialRotation, Time.deltaTime * this._smoothRotation);
    }
}
