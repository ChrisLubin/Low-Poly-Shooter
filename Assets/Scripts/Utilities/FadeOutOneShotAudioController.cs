using UnityEngine;

public class FadeOutOneShotAudioController : MonoBehaviour
{
    private AudioSource _audioSource;

    private float _startFadeAt;
    private float _originalVolume;
    private bool _hasInited = false;

    private void Awake()
    {
        this._audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!this._hasInited) { return; }

        float audioPercentDone = this._audioSource.time / this._audioSource.clip.length;
        if (audioPercentDone >= this._startFadeAt)
            this._audioSource.volume = Mathf.Lerp(this._originalVolume, 0f, Mathf.InverseLerp(this._startFadeAt, 1f, audioPercentDone));
    }

    public void Init(float startFadeAt)
    {
        this._startFadeAt = startFadeAt;
        this._originalVolume = this._audioSource.volume;
        this._hasInited = true;
    }
}
