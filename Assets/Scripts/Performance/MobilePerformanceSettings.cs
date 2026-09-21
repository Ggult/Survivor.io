using UnityEngine;

public static class MobilePerformanceSettings
{
    private const int TargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        Application.targetFrameRate = TargetFrameRate;
        QualitySettings.vSyncCount = 0;
    }
}
