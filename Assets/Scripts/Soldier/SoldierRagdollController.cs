using System;
using System.Threading.Tasks;
using UnityEngine;

public class SoldierRagdollController : MonoBehaviour
{
    [SerializeField] private Transform _ragdollRootBone;
    [SerializeField] private Camera _camera;

    public async void DoRagroll(Transform originalRootBone, bool isLocalPlayer)
    {
        this.MatchAllChildTransform(originalRootBone, this._ragdollRootBone);

        if (!isLocalPlayer) { return; }

        this._camera.enabled = true;
        // Remove this and make SPAWN_PLAYER_REQUEST_TIMER private after doing Cinemachine fix and also auto remove auto spawn and add button that user clicks to spawn
        await UnityTimer.Delay(SoldierManager.SPAWN_PLAYER_REQUEST_TIMER);
        this._camera.enabled = false;
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
}
