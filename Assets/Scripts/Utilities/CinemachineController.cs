using Cinemachine;
using UnityEngine;

public class CinemachineController : MonoBehaviour
{
    private static CinemachineBrain _brain;

    private void Awake() => CinemachineController._brain = GetComponent<CinemachineBrain>();

    public static void SetBlendDuration(float duration) => CinemachineController._brain.m_DefaultBlend.m_Time = duration;
}
