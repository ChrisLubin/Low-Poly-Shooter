using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator _animator;
    private WeaponController _weaponController;

    private const string _IS_ADS_PARAMETER_NAME = "IsADS";

    private void Awake()
    {
        this._animator = GetComponent<Animator>();
        this._weaponController = GetComponent<WeaponController>();
        this._weaponController.OnADS += this.SetADS;
    }

    private void OnDestroy() => this._weaponController.OnADS -= this.SetADS;
    public void SetADS(bool isADS) => this._animator.SetBool(_IS_ADS_PARAMETER_NAME, isADS);
}
