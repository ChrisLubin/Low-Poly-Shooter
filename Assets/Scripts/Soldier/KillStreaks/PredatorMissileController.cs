using System;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PredatorMissileController : NetworkBehaviorAutoDisable<PredatorMissileController>
{
    private NetworkObject _networkObject;

    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private float _movementSpeed = 50f;

    [SerializeField] private float _lookLimit = 100f;
    [SerializeField] private float _lookSpeed = 0.2f;
    private float _rotationX = 0;
    private float _rotationZ = 0;

    private static float _CAMERA_EXIT_TRANSITION_TIME = 2f;
    public static event Action OnLocalPlayerPredatorMissileExploded;

    private void Awake()
    {
        this._networkObject = GetComponent<NetworkObject>();
    }

    protected override void OnOwnerNetworkSpawn()
    {
        CinemachineController.SetBlendDuration(2f);
        this._camera.enabled = true;
    }

    private void Update()
    {
        if (Helpers.WillCollide(transform.position, this.GetNextPosition(), out Vector3 collidePosition))
        {
            this.OnCollision(collidePosition);
            return;
        }

        this._rotationX += -Input.GetAxis("Mouse Y") * this._lookSpeed;
        this._rotationX = Mathf.Clamp(this._rotationX, -this._lookLimit, this._lookLimit);
        this._rotationZ += Input.GetAxis("Mouse X") * this._lookSpeed;
        this._rotationZ = Mathf.Clamp(this._rotationZ, -this._lookLimit, this._lookLimit);
        transform.localRotation = Quaternion.Euler(this._rotationX, 0, this._rotationZ);

        transform.position = this.GetNextPosition();
    }

    private Vector3 GetNextPosition() => transform.position + this._movementSpeed * Time.deltaTime * -transform.up;

    private void OnCollision(Vector3 collisionPosition)
    {
        gameObject.SetActive(false);

        if (this.IsOwner)
        {
            CinemachineController.SetBlendDuration(_CAMERA_EXIT_TRANSITION_TIME);
            PredatorMissileController.OnLocalPlayerPredatorMissileExploded?.Invoke();
            this.OnCollisionServerRpc(collisionPosition);
        }
    }

    [ServerRpc]
    private void OnCollisionServerRpc(Vector3 collisionPosition, ServerRpcParams serverRpcParams = default)
    {
        ulong[] allClientIds = Helpers.ToArray(NetworkManager.Singleton.ConnectedClientsIds);

        // Send to all clients except the owner
        ClientRpcParams rpcParams = new()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = allClientIds.Where((ulong clientId) => clientId != serverRpcParams.Receive.SenderClientId).ToArray()
            }
        };

#pragma warning disable CS4014
        this.DespawnWithDelay(_CAMERA_EXIT_TRANSITION_TIME + 0.5f);
        this.OnCollisionClientRpc(collisionPosition, rpcParams);
    }

    [ClientRpc]
    private void OnCollisionClientRpc(Vector3 collisionPosition, ClientRpcParams _ = default)
    {
        if (this.IsOwner) { return; }

        this.OnCollision(collisionPosition);
    }

    private async UniTask DespawnWithDelay(float delay)
    {
        if (!this.IsHost) { return; }

        await UniTask.WaitForSeconds(delay);
        this._networkObject.Despawn();
    }
}
