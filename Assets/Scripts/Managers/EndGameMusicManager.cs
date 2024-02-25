using Cysharp.Threading.Tasks;
using UnityEngine;

public class EndGameMusicManager : WithLogger<EndGameMusicManager>
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _winningSong;
    [SerializeField] private AudioClip _losingSong;

    private const float _WINNING_SONG_MIN_VOLUME = 0.015f;
    private const float _WINNING_SONG_MAX_VOLUME = 0.09f;
    private const float _LOSING_SONG_MIN_VOLUME = 0.06f;
    private const float _LOSING_SONG_MAX_VOLUME = 0.25f;
    private const float _FADE_MUSIC_DURATION = 1f;

    private bool _isFadingInAndOut = false;
    private float _winningSongTime = 0f;
    private float _losingSongTime = 0f;

    private static AudioSource _AUDIO_SOURCE;

    protected override void Awake()
    {
        base.Awake();
        EndGameMusicManager._AUDIO_SOURCE = this._audioSource;
        ScoreboardController.OnGameNearingEndReached += this.OnGameNearingEndReached;
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
    }

    private void OnDestroy()
    {
        ScoreboardController.OnGameNearingEndReached -= this.OnGameNearingEndReached;
        SoldierManager.OnPlayerDeath -= this.OnPlayerDeath;
    }

    private void Update()
    {
        if (!this._audioSource.isPlaying || this._isFadingInAndOut) { return; }

        // Make end game song louder as game gets closer to ending
        if (this._audioSource.clip == this._winningSong)
        {
            this._audioSource.volume = this.GetAppropriateSongVolume(true);
            this._winningSongTime = this._audioSource.time;
        }
        else if (this._audioSource.clip == this._losingSong)
        {
            this._audioSource.volume = this.GetAppropriateSongVolume(false);
            this._losingSongTime = this._audioSource.time;
        }
    }

    private void OnGameNearingEndReached()
    {
        this._audioSource.clip = ScoreboardController.IsLocalPlayerInFirstPlace() ? this._winningSong : this._losingSong;
        this._audioSource.Play();
        this._logger.Log($"The game is nearing end so playing {(ScoreboardController.IsLocalPlayerInFirstPlace() ? "winning" : "losing")} song!");
    }

    private async void OnPlayerDeath(ulong _, ulong __)
    {
        if (!this._audioSource.isPlaying) { return; }

        if (this._audioSource.clip == this._losingSong && ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._logger.Log("Local player gained the lead in end game.");
            await UniTask.WaitUntil(() => !this._isFadingInAndOut);

            await this.FadeOut();
            await this.FadeIn(this._winningSong, this._winningSongTime, this.GetAppropriateSongVolume(true));
        }
        else if (this._audioSource.clip == this._winningSong && !ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._logger.Log("Local player lost lead in end game.");
            await UniTask.WaitUntil(() => !this._isFadingInAndOut);

            await this.FadeOut();
            await this.FadeIn(this._losingSong, this._losingSongTime, this.GetAppropriateSongVolume(false));
        }
    }

    private float GetAppropriateSongVolume(bool isWinningSong)
    {
        float minVolume = isWinningSong ? _WINNING_SONG_MIN_VOLUME : _LOSING_SONG_MIN_VOLUME;
        float maxVolume = isWinningSong ? _WINNING_SONG_MAX_VOLUME : _LOSING_SONG_MAX_VOLUME;

        return Mathf.Lerp(minVolume, maxVolume, Mathf.InverseLerp(ScoreboardController.NEARING_END_GAME_PERCENT_THRESHOLD, 1f, ScoreboardController.GetGamePercentDone()));
    }

    private async UniTask FadeOut(int steps = 20)
    {
        this._isFadingInAndOut = true;

        float decreaseVolumeStepAmount = this._audioSource.volume / (float)steps;
        float waitTimeBetweenSteps = _FADE_MUSIC_DURATION / (float)steps;

        for (int i = 0; i < steps; i++)
        {
            this._audioSource.volume -= decreaseVolumeStepAmount;
            await UniTask.WaitForSeconds(waitTimeBetweenSteps);
        }
    }

    private async UniTask FadeIn(AudioClip clip, float clipTime, float endVolume, int steps = 20)
    {
        this._audioSource.clip = clip;
        this._audioSource.time = clipTime;
        this._audioSource.volume = 0f;
        this._audioSource.Play();

        float increaseVolumeStepAmount = endVolume / (float)steps;
        float waitTimeBetweenSteps = _FADE_MUSIC_DURATION / (float)steps;

        for (int i = 0; i < steps; i++)
        {
            this._audioSource.volume += increaseVolumeStepAmount;
            await UniTask.WaitForSeconds(waitTimeBetweenSteps);
        }

        this._isFadingInAndOut = false;
    }

    public static bool IsPlayingMusic() => EndGameMusicManager._AUDIO_SOURCE.isPlaying;
    public static bool ToggleMute() => EndGameMusicManager._AUDIO_SOURCE.mute = !EndGameMusicManager._AUDIO_SOURCE.mute;
    public static bool IsMuted() => EndGameMusicManager._AUDIO_SOURCE.mute;
}
