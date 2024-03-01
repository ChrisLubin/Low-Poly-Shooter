using System.Linq;
using UnityEngine;
using static SoldierDamageController;

public class InvisibilityAbilityController : AbilityController
{
    private SoldierDamageController _damageController;
    private WeaponController _weaponController;

    [SerializeField] private Material _semiTransparentMaterial;
    private Renderer[] _meshes;
    private Material[] _originalMaterials;

    private void Awake()
    {
        this.Ability = Abilities.Invisibility;
        this._damageController = GetComponent<SoldierDamageController>();
        this._damageController.OnPlayerDamagedByLocalPlayer += this.OnPlayerDamagedByLocalPlayer;
        this._damageController.OnServerDamageReceived += this.OnPlayerReceivedServerDamage;
        this._weaponController = GetComponentInChildren<WeaponController>();
        this._weaponController.OnShoot += this.OnPlayerDidShoot;
        this._meshes = GetComponentsInChildren<Renderer>();
        this._originalMaterials = this._meshes.Select(mesh => mesh.materials[0]).ToArray();
    }

    private void OnDestroy()
    {
        this._damageController.OnPlayerDamagedByLocalPlayer -= this.OnPlayerDamagedByLocalPlayer;
        this._damageController.OnServerDamageReceived -= this.OnPlayerReceivedServerDamage;
        this._weaponController.OnShoot -= this.OnPlayerDidShoot;
    }

    public override void Activate()
    {
        base.Activate();

        this.ChangeMeshes(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();

        this.ChangeMeshes(false);
    }

    private void ChangeMeshes(bool isTurningInvisible)
    {
        for (int i = 0; i < this._meshes.Count(); i++)
        {
            Renderer renderer = this._meshes[i];
            Material[] materialArray = new Material[1];

            if (isTurningInvisible)
                materialArray[0] = this._semiTransparentMaterial;
            else
                materialArray[0] = this._originalMaterials[i];

            renderer.materials = materialArray;
        }
    }

    private void OnPlayerDamagedByLocalPlayer(Vector3 _, DamageType __, int ___) => this.TryInternallyDeactivate();
    private void OnPlayerReceivedServerDamage(DamageType _, int __) => this.TryInternallyDeactivate();
    private void OnPlayerDidShoot() => this.TryInternallyDeactivate();

    private bool TryInternallyDeactivate()
    {
        if (this.IsActive)
        {
            this.Deactivate();
            base.DeactivateInternally();
            return true;
        }

        return false;
    }
}
