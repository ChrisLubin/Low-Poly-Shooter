using System;
using Unity.Netcode.Components;
using UnityEngine;

public class WeaponAnimationController : MonoBehaviour
{
    private Animator _animator;
    private NetworkAnimator _networkAnimator;
    private WeaponController _weaponController;
    private WeaponAmmoController _ammoController;

    private const string _IS_ADS_PARAMETER_NAME = "IsADS";
    private const string _DO_RELOAD_PARAMETER_NAME = "DoReload";
    private const string _IS_RELOADING_PARAMETER_NAME = "IsReloading";

    public bool IsReloading => this._animator.GetBool(_IS_RELOADING_PARAMETER_NAME);

    public event Action OnReloadDone;

    [SerializeField] private AudioSource _reloadAudioSource;
    [SerializeField] private AudioClip _reloadMagazineOutSoundEffect;
    [SerializeField] private AudioClip _reloadMagazineInSoundEffect;
    private const float _RELOAD_MAGAZINE_AUDIO_VOLUME = 0.15f;

    private void Awake()
    {
        this._animator = GetComponent<Animator>();
        this._weaponController = GetComponent<WeaponController>();
        this._ammoController = GetComponent<WeaponAmmoController>();
        this._networkAnimator = GetComponent<NetworkAnimator>();
        this._weaponController.OnADS += this.SetADS;
        this._ammoController.OnReloadRequest += OnReloadRequest;
        this._animator.keepAnimatorStateOnDisable = true;
    }

    private void OnDestroy()
    {
        this._weaponController.OnADS -= this.SetADS;
        this._ammoController.OnReloadRequest -= OnReloadRequest;
    }

    private void SetADS(bool isADS) => this._animator.SetBool(_IS_ADS_PARAMETER_NAME, isADS);
    private void OnReloadRequest() => this._networkAnimator.SetTrigger(_DO_RELOAD_PARAMETER_NAME);

    // Called by animation events
    public void _OnReloadDone()
    {
        this._animator.SetBool(_IS_RELOADING_PARAMETER_NAME, false);
        this.OnReloadDone?.Invoke();
    }

    public void _OnReloadMagazineOut()
    {
        this._reloadAudioSource.clip = this._reloadMagazineOutSoundEffect;
        this._reloadAudioSource.volume = _RELOAD_MAGAZINE_AUDIO_VOLUME;
        this._reloadAudioSource.Play();
    }

    public void _OnReloadMagazineIn()
    {
        this._reloadAudioSource.clip = this._reloadMagazineInSoundEffect;
        this._reloadAudioSource.volume = _RELOAD_MAGAZINE_AUDIO_VOLUME;
        this._reloadAudioSource.Play();
    }
}
