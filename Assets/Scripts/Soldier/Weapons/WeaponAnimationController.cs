using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator _animator;
    private const string _IS_ADS_PARAMETER_NAME = "IsADS";
    private const int _ADS_ANIMATION_DEFAULT_TIME_MILLISECONDS = 500;
    private const float _ADS_ANIMATION_SPEED_MULTIPLIER = 1.7f;
    private const float _ADS_ANIMATION_TIME_MILLISECONDS = _ADS_ANIMATION_DEFAULT_TIME_MILLISECONDS / _ADS_ANIMATION_SPEED_MULTIPLIER;
    private bool _isADS;
    private float _timeSinceLastADSAnimation = Mathf.Infinity;

    private void Awake() => this._animator = GetComponent<Animator>();
    private void Start() => this._isADS = false;

    void Update()
    {
        // Can't switch ADS during animation
        this._timeSinceLastADSAnimation += Time.deltaTime * 1000;
        if (this._timeSinceLastADSAnimation < _ADS_ANIMATION_TIME_MILLISECONDS) { return; }

        if (Input.GetMouseButtonDown(1))
        {
            this._isADS = !this._isADS;
            this._animator.SetBool(_IS_ADS_PARAMETER_NAME, this._isADS);
            this._timeSinceLastADSAnimation = 0f;
        }
    }
}
