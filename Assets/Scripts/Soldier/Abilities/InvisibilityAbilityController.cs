using System.Linq;
using UnityEngine;

public class InvisibilityAbilityController : AbilityController
{
    [SerializeField] private Material _semiTransparentMaterial;
    private Renderer[] _meshes;
    private Material[] _originalMaterials;

    private void Awake()
    {
        this.Ability = Abilities.Invisibility;
        this._meshes = GetComponentsInChildren<Renderer>();
        this._originalMaterials = this._meshes.Select(mesh => mesh.materials[0]).ToArray();
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
}
