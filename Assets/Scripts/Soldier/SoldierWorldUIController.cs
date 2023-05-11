using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SoldierWorldUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshPro _playerNameText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (this.IsOwner)
        {
            this._playerNameText.gameObject.SetActive(false);
            this.enabled = false;
            return;
        }

        this._playerNameText.text = MultiplayerSystem.Instance.GetPlayerUsername(this.OwnerClientId);
    }
}
