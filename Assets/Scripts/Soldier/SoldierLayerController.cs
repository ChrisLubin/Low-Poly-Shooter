using Unity.Netcode;
using UnityEngine;

public class SoldierLayerController : NetworkBehaviour
{
    private Renderer[] _meshes;

    private void Awake()
    {
        this._meshes = GetComponentsInChildren<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (this.IsOwner) { return; }

        foreach (Renderer mesh in this._meshes)
            mesh.gameObject.layer = LayerMask.NameToLayer(Constants.LayerNames.Enemy);
    }
}
