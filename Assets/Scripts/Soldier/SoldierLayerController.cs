using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class SoldierLayerController : NetworkBehaviour
{
    private Renderer[] _meshes;

    private void Awake()
    {
        this._meshes = GetComponentsInChildren<Renderer>();
    }

    public async override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsOwner)
            await this.SetEnemyLayerWithDelay();
    }

    private async UniTask SetEnemyLayerWithDelay()
    {
        await UniTask.WaitForSeconds(SoldierCameraController.SOLDIER_SPAWN_CAMERA_TRANSITION_TIME + 3f);

        foreach (Renderer mesh in this._meshes)
            mesh.gameObject.layer = LayerMask.NameToLayer(Constants.LayerNames.Enemy);
    }
}
