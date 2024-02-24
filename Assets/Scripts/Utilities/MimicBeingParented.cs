using UnityEngine;

public class MimicBeingParented : MonoBehaviour
{

    [SerializeField] private Transform _parentTransform;

    private Vector3 _startParentPosition;
    private Quaternion _startParentRotationQ;
    private Vector3 _startParentScale;

    private Vector3 _startChildPosition;
    private Quaternion _startChildRotationQ;

    private Matrix4x4 _parentMatrix;

    void Start()
    {

        this._startParentPosition = this._parentTransform.position;
        this._startParentRotationQ = this._parentTransform.rotation;
        this._startParentScale = this._parentTransform.lossyScale;

        this._startChildPosition = transform.position;
        this._startChildRotationQ = transform.rotation;

        // Keeps child position from being modified at the start by the parent's initial transform
        this._startChildPosition = DivideVectors(Quaternion.Inverse(this._parentTransform.rotation) * (this._startChildPosition - this._startParentPosition), this._startParentScale);
    }

    void Update()
    {

        this._parentMatrix = Matrix4x4.TRS(this._parentTransform.position, this._parentTransform.rotation, this._parentTransform.lossyScale);

        transform.position = this._parentMatrix.MultiplyPoint3x4(this._startChildPosition);

        transform.rotation = (this._parentTransform.rotation * Quaternion.Inverse(this._startParentRotationQ)) * this._startChildRotationQ;
    }

    Vector3 DivideVectors(Vector3 num, Vector3 den)
    {

        return new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);

    }
}
