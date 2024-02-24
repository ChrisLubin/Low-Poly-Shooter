using Cinemachine;
using UnityEngine;

public class SoldierRagdollController : MonoBehaviour
{
    [SerializeField] private Transform _ragdollRootBone;
    [SerializeField] private CinemachineVirtualCamera _camera;

    private void Awake()
    {
        GameManager.OnStateChange += this.OnGameStateChange;
        SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;

    }
    private void OnDestroy()
    {
        GameManager.OnStateChange -= this.OnGameStateChange;
        SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
    }

    public void DoRagroll(Transform originalRootBone, bool isLocalPlayer)
    {
        this.MatchAllChildTransform(originalRootBone, this._ragdollRootBone);

        if (!isLocalPlayer || GameManager.State == GameState.GameOver) { return; }

        this._camera.enabled = true;
    }

    private void MatchAllChildTransform(Transform root, Transform clone)
    {
        foreach (Transform originalChild in root)
        {
            Transform cloneChild = clone.Find(originalChild.name);
            if (cloneChild == null)
            {
                continue;
            }

            cloneChild.position = originalChild.position;
            cloneChild.rotation = originalChild.rotation;
            MatchAllChildTransform(originalChild, cloneChild);
        }
    }

    private void OnLocalPlayerSpawn() => this._camera.enabled = false;

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                this._camera.enabled = false;
                break;
            default:
                break;
        }
    }
}
