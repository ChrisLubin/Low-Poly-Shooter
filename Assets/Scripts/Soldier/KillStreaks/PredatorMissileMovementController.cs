using System;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PredatorMissileMovementController : NetworkBehaviorAutoDisable<PredatorMissileMovementController>
{
    private NetworkObject _networkObject;

    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private float _movementSpeed = 50f;

    [SerializeField] private float _lookLimit = 100f;
    [SerializeField] private float _lookSpeed = 0.2f;
    private float _rotationX = 0;
    private float _rotationZ = 0;

    private const float _CAMERA_EXIT_TRANSITION_TIME = 2f;
    public const float AUTO_EXPLODE_TIME = 15f;
    public static float AUTO_EXPLODE_TIMER { get; private set; } = 0f;
    public event Action<Vector3> OnExploded;

    private void Awake()
    {
        this._networkObject = GetComponent<NetworkObject>();
    }

    protected override void OnOwnerNetworkSpawn()
    {
        AUTO_EXPLODE_TIMER = 0f;
        CinemachineController.SetBlendDuration(2f);
        this._camera.enabled = true;
    }

    private void Update()
    {
        AUTO_EXPLODE_TIMER += Time.deltaTime;

        if (AUTO_EXPLODE_TIMER >= AUTO_EXPLODE_TIME)
        {
            this.OnExplode(transform.position);
            return;
        }
        if (Helpers.WillCollide(transform.position, this.GetNextPosition(), out Vector3 collidePosition, out _))
        {
            this.OnExplode(collidePosition);
            return;
        }

        if (!PauseMenuController.IsPaused)
        {
            this._rotationX += -Input.GetAxis("Mouse Y") * this._lookSpeed;
            this._rotationX = Mathf.Clamp(this._rotationX, -this._lookLimit, this._lookLimit);
            this._rotationZ += Input.GetAxis("Mouse X") * this._lookSpeed;
            this._rotationZ = Mathf.Clamp(this._rotationZ, -this._lookLimit, this._lookLimit);
            transform.localRotation = Quaternion.Euler(this._rotationX, 0, this._rotationZ);
        }

        transform.position = this.GetNextPosition();
    }

    private Vector3 GetNextPosition() => transform.position + this._movementSpeed * Time.deltaTime * -transform.up;

    private void OnExplode(Vector3 explodePosition)
    {
        gameObject.SetActive(false);

        if (this.IsOwner)
        {
            AUTO_EXPLODE_TIMER = 0f;
            CinemachineController.SetBlendDuration(_CAMERA_EXIT_TRANSITION_TIME);
            this.OnExplodeServerRpc(explodePosition);
        }

        this.OnExploded?.Invoke(explodePosition);
    }

    [ServerRpc]
    private void OnExplodeServerRpc(Vector3 explodePosition, ServerRpcParams serverRpcParams = default)
    {
#pragma warning disable CS4014
        this.DespawnWithDelay(_CAMERA_EXIT_TRANSITION_TIME + 0.5f);
        this.OnExplodeClientRpc(explodePosition, serverRpcParams.GetClientRpcParamsWithoutSender());
    }

    [ClientRpc]
    private void OnExplodeClientRpc(Vector3 explodePosition, ClientRpcParams _ = default)
    {
        if (this.IsOwner) { return; }

        this.OnExplode(explodePosition);
    }

    private async UniTask DespawnWithDelay(float delay)
    {
        if (!this.IsHost) { return; }

        await UniTask.WaitForSeconds(delay);
        this._networkObject.Despawn();
    }
}
