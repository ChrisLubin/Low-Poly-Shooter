using UnityEngine;

public class SoldierController : NetworkBehaviorAutoDisable<SoldierController>
{
    [SerializeField] private GameObject _playerCamera;

    protected override void OnOwnerNetworkSpawn()
    {
        this._playerCamera.GetComponent<Camera>().enabled = true;
    }
}
