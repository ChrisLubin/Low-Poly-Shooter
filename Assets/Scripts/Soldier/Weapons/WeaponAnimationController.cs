using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator _animator;
    private WeaponController _weaponController;

    private const string _IS_ADS_PARAMETER_NAME = "IsADS";
    private const int _ADS_ANIMATION_DEFAULT_TIME_MILLISECONDS = 500;
    private const float _ADS_ANIMATION_SPEED_MULTIPLIER = 1.7f;
    public const float ADS_ANIMATION_TIME_MILLISECONDS = _ADS_ANIMATION_DEFAULT_TIME_MILLISECONDS / _ADS_ANIMATION_SPEED_MULTIPLIER;

    private void Awake()
    {
        this._animator = GetComponent<Animator>();
        this._weaponController = GetComponent<WeaponController>();
        this._weaponController.OnADS += this.SetADS;
    }

    private void OnDestroy() => this._weaponController.OnADS -= this.SetADS;
    public void SetADS(bool isADS) => this._animator.SetBool(_IS_ADS_PARAMETER_NAME, isADS);
}
