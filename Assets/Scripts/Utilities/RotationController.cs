using UnityEngine;

public class RotationController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;

    private void Update() => transform.Rotate(0f, this._rotationSpeed * Time.deltaTime, 0f);
}
