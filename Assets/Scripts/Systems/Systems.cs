using UnityEngine;

public class Systems : PersistentSingleton<Systems>
{
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Debug.isDebugBuild ? 45 : 120;
    }

    protected override void OnApplicationQuit()
    {
#if !UNITY_EDITOR
    System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }
}
