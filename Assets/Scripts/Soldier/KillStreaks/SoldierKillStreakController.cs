using System;
using Unity.Netcode;
using UnityEngine;

public class SoldierKillStreakController : NetworkBehaviorAutoDisableWithLogger<SoldierKillStreakController>
{
    [SerializeField] private Transform _predatorMissilePrefab;

    private static bool _HAS_KILL_STREAK = false;
    public static bool IS_USING_KILL_STREAK { get; private set; } = false;
    private static int _KILL_STEAK_COUNT = 0;
    public const int KILLS_NEEDED_FOR_PREDATOR_MISSILE = 3;

    public event Action OnUseKillStreak;
    public static event Action<int> OnLocalPlayerKillStreakCountChange;
    public static event Action<bool> OnLocalPlayerKillStreakActivatedOrDeactivated;

    protected override void OnOwnerNetworkSpawn()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        PredatorMissileDamageController.OnLocalPlayerMissileExploded += this.OnLocalPlayerMissileExploded;
        SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (this.IsOwner)
        {
            GameManager.OnStateChange -= this.OnGameStateChange;
            PredatorMissileDamageController.OnLocalPlayerMissileExploded -= this.OnLocalPlayerMissileExploded;
            SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
            SoldierManager.OnPlayerDeath -= this.OnPlayerDeath;
        }
    }

    private void Update()
    {
        if (PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || !SoldierKillStreakController._HAS_KILL_STREAK || SoldierKillStreakController.IS_USING_KILL_STREAK || _KILL_STEAK_COUNT < KILLS_NEEDED_FOR_PREDATOR_MISSILE || !Input.GetKeyDown(KeyCode.K)) { return; }

        this._logger.Log($"Local player requested predator missile");
        this.SpawnPredatorMissileServerRpc();
        SoldierKillStreakController._HAS_KILL_STREAK = false;
        SoldierKillStreakController.IS_USING_KILL_STREAK = true;
        this.OnUseKillStreak?.Invoke();
        OnLocalPlayerKillStreakActivatedOrDeactivated?.Invoke(true);
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarted:
                SoldierKillStreakController._HAS_KILL_STREAK = false;
                SoldierKillStreakController.IS_USING_KILL_STREAK = false;
                SoldierKillStreakController._KILL_STEAK_COUNT = 0;
                break;
            default:
                break;
        }
    }

    [ServerRpc]
    private void SpawnPredatorMissileServerRpc()
    {
        Transform playerTransform = Instantiate(this._predatorMissilePrefab, KillStreakManager.Instance.GetPredatorMissileSpawnPoint(), Quaternion.identity);
        playerTransform.GetComponent<NetworkObject>().SpawnWithOwnership(this.OwnerClientId);
        this._logger.Log($"Spawned predator missile for {MultiplayerSystem.Instance.GetPlayerUsername(this.OwnerClientId)}");
    }

    private void OnLocalPlayerMissileExploded()
    {
        SoldierKillStreakController.IS_USING_KILL_STREAK = false;
        SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated?.Invoke(false);
    }

    private void OnLocalPlayerDeath()
    {
        if (SoldierKillStreakController._KILL_STEAK_COUNT < KILLS_NEEDED_FOR_PREDATOR_MISSILE || !SoldierKillStreakController._HAS_KILL_STREAK)
        {
            SoldierKillStreakController._KILL_STEAK_COUNT = 0;
            SoldierKillStreakController.OnLocalPlayerKillStreakCountChange?.Invoke(_KILL_STEAK_COUNT);
        }
    }

    private void OnPlayerDeath(ulong deadClientId, ulong killerClientId, DamageType _)
    {
        if (!this.IsOwner || killerClientId != NetworkManager.Singleton.LocalClientId || killerClientId == deadClientId) { return; }

        SoldierKillStreakController._KILL_STEAK_COUNT++;

        if (SoldierKillStreakController._KILL_STEAK_COUNT == SoldierKillStreakController.KILLS_NEEDED_FOR_PREDATOR_MISSILE)
            SoldierKillStreakController._HAS_KILL_STREAK = true;

        OnLocalPlayerKillStreakCountChange?.Invoke(SoldierKillStreakController._KILL_STEAK_COUNT);
    }
}
