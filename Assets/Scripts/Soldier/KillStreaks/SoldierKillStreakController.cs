using Unity.Netcode;
using UnityEngine;

public class SoldierKillStreakController : NetworkBehaviorAutoDisableWithLogger<SoldierKillStreakController>
{
    [SerializeField] private Transform _predatorMissilePrefab;

    private static bool _HAS_KILL_STREAK = true;
    public static bool IS_USING_KILL_STREAK { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnStateChange += this.OnGameStateChange;
        PredatorMissileController.OnLocalPlayerPredatorMissileExploded += this.OnLocalPlayerPredatorMissileExploded;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.OnStateChange -= this.OnGameStateChange;
        PredatorMissileController.OnLocalPlayerPredatorMissileExploded -= this.OnLocalPlayerPredatorMissileExploded;
    }

    private void Update()
    {
        if (PauseMenuController.IsPaused || GameManager.State == GameState.GameOver || !SoldierKillStreakController._HAS_KILL_STREAK || SoldierKillStreakController.IS_USING_KILL_STREAK || !Input.GetKeyDown(KeyCode.K)) { return; }

        this._logger.Log($"Local player requested predator missile");
        this.SpawnPredatorMissileServerRpc();
        SoldierKillStreakController.IS_USING_KILL_STREAK = true;
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarted:
                SoldierKillStreakController.IS_USING_KILL_STREAK = false;
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

    private void OnLocalPlayerPredatorMissileExploded() => SoldierKillStreakController.IS_USING_KILL_STREAK = false;
}
