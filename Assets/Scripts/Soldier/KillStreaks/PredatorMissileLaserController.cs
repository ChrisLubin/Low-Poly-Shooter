using Unity.Netcode;
using UnityEngine;

public class PredatorMissileLaserController : NetworkBehaviour
{
    [SerializeField] private LineRenderer _laser;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsOwner)
            this._laser.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (this.IsOwner) { return; }

        Vector3 laserEndPoint = this._laser.transform.position + (this._laser.transform.forward * 200f);

        if (Physics.Raycast(this._laser.transform.position, this._laser.transform.forward, out RaycastHit hit))
            laserEndPoint = hit.point;

        this._laser.SetPosition(0, this._laser.transform.position);
        this._laser.SetPosition(1, laserEndPoint);
    }
}
