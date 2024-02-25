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

    private float _winningSongTime = 0f;
    private float _losingSongTime = 0f;

    protected override void Awake()
    {
        base.Awake();
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
        if (!this._audioSource.isPlaying) { return; }

        // Make end game song louder as game gets closer to ending
        if (this._audioSource.clip == this._winningSong)
        {
            this._audioSource.volume = Mathf.Lerp(_WINNING_SONG_MIN_VOLUME, _WINNING_SONG_MAX_VOLUME, Mathf.InverseLerp(ScoreboardController.NEARING_END_GAME_PERCENT_THRESHOLD, 1f, ScoreboardController.GetGamePercentDone()));
            this._winningSongTime = this._audioSource.time;
        }
        else if (this._audioSource.clip == this._losingSong)
        {
            this._audioSource.volume = Mathf.Lerp(_LOSING_SONG_MIN_VOLUME, _LOSING_SONG_MAX_VOLUME, Mathf.InverseLerp(ScoreboardController.NEARING_END_GAME_PERCENT_THRESHOLD, 1f, ScoreboardController.GetGamePercentDone()));
            this._losingSongTime = this._audioSource.time;
        }
    }

    private void OnGameNearingEndReached()
    {
        this._audioSource.clip = ScoreboardController.IsLocalPlayerInFirstPlace() ? this._winningSong : this._losingSong;
        this._audioSource.Play();
        this._logger.Log($"The game is nearing end so playing {(ScoreboardController.IsLocalPlayerInFirstPlace() ? "winning" : "losing")} song!");
    }

    private void OnPlayerDeath(ulong _, ulong __)
    {
        if (!this._audioSource.isPlaying) { return; }

        if (this._audioSource.clip == this._losingSong && ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._logger.Log("Local player gained the lead in end game.");
            this._audioSource.clip = this._winningSong;
            this._audioSource.time = this._winningSongTime;
            this._audioSource.Play();
        }
        else if (this._audioSource.clip == this._winningSong && !ScoreboardController.IsLocalPlayerInFirstPlace())
        {
            this._logger.Log("Local player lost lead in end game.");
            this._audioSource.clip = this._losingSong;
            this._audioSource.time = this._losingSongTime;
            this._audioSource.Play();
        }
    }
}
