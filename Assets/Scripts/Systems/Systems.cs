using UnityEngine;

public class Systems : PersistentSingleton<Systems>
{
    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Debug.isDebugBuild ? 45 : 120;
    }
}
