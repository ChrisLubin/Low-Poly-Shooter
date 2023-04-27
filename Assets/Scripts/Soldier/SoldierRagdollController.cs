using UnityEngine;

public class SoldierRagdollController : MonoBehaviour
{
    [SerializeField] private Transform _ragdollRootBone;
    [SerializeField] private Camera _camera;

    public void DoRagroll(Transform originalRootBone, bool isLocalPlayer)
    {
        if (isLocalPlayer)
        {
            this._camera.enabled = true;
        }

        this.MatchAllChildTransform(originalRootBone, this._ragdollRootBone);
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
