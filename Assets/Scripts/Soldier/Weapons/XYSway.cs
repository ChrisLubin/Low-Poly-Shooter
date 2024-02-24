using UnityEngine;

public class XYSway : NetworkBehaviorAutoDisable<XYSway>
{
    [SerializeField] private float _intensity = 1;
    [SerializeField] private float _smooth = 10;
    private Quaternion _originRotation;

    private void Start() => this._originRotation = transform.localRotation;
    private void Update() => UpdateSway();

    private void UpdateSway()
    {
        if (PauseMenuController.IsPaused || GameManager.State == GameState.GameOver) { return; }

        float mouseXAxisDelta = Input.GetAxis("Mouse X");
        float mouseYAxisDelta = Input.GetAxis("Mouse Y");

        // Calculate target rotation
        Quaternion targetXRotation = Quaternion.AngleAxis(-this._intensity * mouseXAxisDelta, Vector3.up);
        Quaternion targetYRotation = Quaternion.AngleAxis(this._intensity * mouseYAxisDelta, Vector3.right);
        Quaternion targetRotation = this._originRotation * targetXRotation * targetYRotation;

        // Rotate towards target rotation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * this._smooth);
    }
}
