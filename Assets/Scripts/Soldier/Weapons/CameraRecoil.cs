using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    [Header("Recoil Strength")]
    [SerializeField] private float _recoilX;
    [SerializeField] private float _recoilY;
    [SerializeField] private float _recoilZ;

    [Header("Settings")]
    [SerializeField] private float _snappiness;
    [SerializeField] private float _returnSpeed;

    private float _timeSinceLastShot = Mathf.Infinity;
    private const float _MIN_TIME_BETWEEN_SHOTS = 85.71f; // 700 RPM

    void Update()
    {
        this._timeSinceLastShot += Time.deltaTime * 1000;
        this._targetRotation = Vector3.Lerp(this._targetRotation, Vector3.zero, this._returnSpeed * Time.deltaTime);
        this._currentRotation = Vector3.Slerp(this._currentRotation, this._targetRotation, this._snappiness * Time.fixedDeltaTime);

        transform.localRotation = Quaternion.Euler(this._currentRotation);

        if (Input.GetMouseButton(0) && this._timeSinceLastShot > _MIN_TIME_BETWEEN_SHOTS)
        {
            Fire();
            this._timeSinceLastShot = 0f;
        }
    }

    private void Fire()
    {
        this._targetRotation += new Vector3(this._recoilX, Random.Range(-this._recoilY, this._recoilY), Random.Range(-this._recoilZ, this._recoilZ));
    }
}
