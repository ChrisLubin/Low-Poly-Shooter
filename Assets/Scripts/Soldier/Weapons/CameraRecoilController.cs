using UnityEngine;

public class CameraRecoilController : MonoBehaviour
{
    [SerializeField] private WeaponController _weaponController;

    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    private float _recoilX = 0f;
    private float _recoilY = 0f;
    private float _recoilZ = 0f;
    private float _snappiness = 0f;
    private float _returnSpeed = 0f;

    private void Awake()
    {
        this._recoilX = this._weaponController.RecoilX;
        this._recoilY = this._weaponController.RecoilY;
        this._recoilZ = this._weaponController.RecoilZ;
        this._snappiness = this._weaponController.Snappiness;
        this._returnSpeed = this._weaponController.ReturnSpeed;
        this._weaponController.OnShoot += this.DoRecoil;
    }

    private void OnDestroy()
    {
        this._weaponController.OnShoot -= this.DoRecoil;
    }

    void Update()
    {
        if (this._currentRotation == this._targetRotation) { return; }

        this._targetRotation = Vector3.Lerp(this._targetRotation, Vector3.zero, this._returnSpeed * Time.deltaTime);
        this._currentRotation = Vector3.Slerp(this._currentRotation, this._targetRotation, this._snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(this._currentRotation);
    }

    private void DoRecoil() => this._targetRotation += new Vector3(this._recoilX, Random.Range(-this._recoilY, this._recoilY), Random.Range(-this._recoilZ, this._recoilZ));
}
