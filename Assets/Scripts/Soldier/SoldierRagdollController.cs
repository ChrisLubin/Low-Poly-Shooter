using Cinemachine;
using UnityEngine;

public class SoldierRagdollController : MonoBehaviour
{
    [SerializeField] private Transform _rootBone;
    [SerializeField] private CinemachineVirtualCamera _camera;

    [Header("Missile Blast Properties")]
    [SerializeField] private float _missileBlastForce = 2000f;
    [SerializeField] private float _missileBlastRange = 15f;
    [SerializeField] private float _blastYOffset = 0.2f;

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

    public void DoRagroll(Transform originalRootBone, bool isLocalPlayer, Vector3 damagePoint, DamageType damageType)
    {
        this.MatchAllChildTransform(originalRootBone, this._rootBone);

        if (damageType == DamageType.Missile)
        {
            Vector3 directionToBlast = (new Vector3(damagePoint.x, 0f, damagePoint.z) - new Vector3(transform.position.x, 0f, transform.position.z)).normalized;
            Vector3 modifiedDamagePoint = transform.position + (directionToBlast - new Vector3(0f, this._blastYOffset, 0f));
            this.ApplyExplosion(this._rootBone, this._missileBlastForce, modifiedDamagePoint, this._missileBlastRange);
        }

        if (!isLocalPlayer || GameManager.State == GameState.GameOver) { return; }

        if (damageType == DamageType.Missile)
            CinemachineController.SetBlendDuration(0.2f);

        this._camera.enabled = true;
    }

    private void MatchAllChildTransform(Transform root, Transform clone)
    {
        foreach (Transform originalChild in root)
        {
            Transform cloneChild = clone.Find(originalChild.name);
            if (cloneChild == null) { continue; }

            cloneChild.position = originalChild.position;
            cloneChild.rotation = originalChild.rotation;
            MatchAllChildTransform(originalChild, cloneChild);
        }
    }

    private void ApplyExplosion(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidBody))
            {
                childRigidBody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }

            this.ApplyExplosion(child, explosionForce, explosionPosition, explosionRange);
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
