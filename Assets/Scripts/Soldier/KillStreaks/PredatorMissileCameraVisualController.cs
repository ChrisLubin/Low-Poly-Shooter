using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;

public class PredatorMissileCameraVisualController : NetworkBehaviour
{
    private PredatorMissileMovementController _movementController;
    private VolumeComponent _monochromeVolumeComponent;

    [SerializeField] private Material _enemySilhouette;
    [SerializeField] private RenderObjects _enemyHiddenRenderer;
    [SerializeField] private RenderObjects _enemyVisibleRenderer;
    [SerializeField] private VolumeProfile _volumeProfile;

    private void Awake()
    {
        this._movementController = GetComponent<PredatorMissileMovementController>();
        this._movementController.OnExploded += this.OnExploded;
        this._monochromeVolumeComponent = this._volumeProfile.components.FirstOrDefault(component => component.name == "ColorAdjustments");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this._movementController.OnExploded -= this.OnExploded;

        if (!this.IsOwner) { return; }
        this.ResetVisualChanges();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!this.IsOwner) { return; }

        this._enemyHiddenRenderer.settings.overrideMaterial = this._enemySilhouette;
        this._enemyVisibleRenderer.settings.overrideMaterial = this._enemySilhouette;
        this._enemyHiddenRenderer.SetActive(true);
        this._enemyVisibleRenderer.SetActive(true);
        this._enemyHiddenRenderer.Create();
        this._enemyVisibleRenderer.Create();
        this._monochromeVolumeComponent.active = true;
    }

    private void OnExploded(Vector3 _)
    {
        if (!this.IsOwner) { return; }

        this.ResetVisualChanges();
    }

    private void ResetVisualChanges()
    {
        this._enemyHiddenRenderer.settings.overrideMaterial = default;
        this._enemyVisibleRenderer.settings.overrideMaterial = default;
        this._enemyHiddenRenderer.SetActive(false);
        this._enemyVisibleRenderer.SetActive(true);
        this._enemyHiddenRenderer.Create();
        this._enemyVisibleRenderer.Create();
        this._monochromeVolumeComponent.active = false;
    }
}
