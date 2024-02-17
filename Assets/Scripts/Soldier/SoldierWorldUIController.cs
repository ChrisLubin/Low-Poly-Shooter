using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SoldierWorldUIController : NetworkBehaviour
{
    [SerializeField] private TextMeshPro _playerNameText;
    [SerializeField] private TextMeshPro _healthText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (this.IsOwner)
        {
            this._playerNameText.gameObject.SetActive(false);
            this._healthText.gameObject.SetActive(false);
            return;
        }
        else
            this._playerNameText.text = MultiplayerSystem.Instance.GetPlayerUsername(this.OwnerClientId);
    }
}
