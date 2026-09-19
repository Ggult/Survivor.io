using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public sealed class RuntimePerformanceTelemetry : MonoBehaviour
{
    private const float GameplayPollInterval = 0.25f;
    private const float MemorySampleInterval = 1f;
    private const float OverlayRefreshInterval = 0.25f;
    private const string ReportFileName = "Performance_Android_Baseline.md";

    private readonly HashSet<int> previousActiveEnemyIds = new HashSet<int>();
    private readonly HashSet<int> currentActiveEnemyIds = new HashSet<int>();

    private GameFlow gameFlow;
    private DifficultyController difficultyController;
    private EnemySpawner enemySpawner;
    private bool sessionActive;
    private bool reportWritten;
    private float sessionStartTime;
    private float playingTime;
    private float nextGameplayPollTime;
    private float nextMemorySampleTime;
    private float nextOverlayRefreshTime;
    private float frameTimeSum;
    private float minimumFrameTime = float.PositiveInfinity;
    private float maximumFrameTime;
    private int frameCount;
    private int peakActiveEnemies;
    private int totalEnemySpawns;
    private int totalEnemyDeaths;
    private int totalPoolGets;
    private int totalPoolReturns;
    private int lastActiveEnemyCount;
    private long startingManagedMemory;
    private long endingManagedMemory;
    private long startingGcAllocatedBytes;
    private long endingGcAllocatedBytes;
    private int startingGcCollections;
    private int endingGcCollections;
    private long totalAllocatedMemory;
    private long totalReservedMemory;
    private long graphicsMemory;
    private float currentFps;
    private float currentFrameTime;
    private float displayedManagedMemory;
    private float displayedGcAllocations;
    private string currentDifficulty = "NOT AVAILABLE";
    private string currentState = "Waiting";
    private GUIStyle overlayStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<RuntimePerformanceTelemetry>() != null)
        {
            return;
        }

        GameObject telemetryObject = new GameObject("RuntimePerformanceTelemetry");
        DontDestroyOnLoad(telemetryObject);
        telemetryObject.AddComponent<RuntimePerformanceTelemetry>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EnemyHealth.Died += HandleEnemyDied;
    }

    private void OnDestroy()
    {
        EnemyHealth.Died -= HandleEnemyDied;
    }

    private void Update()
    {
        ResolveReferences();
        RecordFrameSample();

        if (gameFlow == null)
        {
            return;
        }

        if (!sessionActive && gameFlow.State == GameState.Playing)
        {
            StartSession();
        }

        if (!sessionActive)
        {
            currentState = gameFlow.State.ToString();
            return;
        }

        playingTime += Time.unscaledDeltaTime;
        currentState = gameFlow.State.ToString();
        currentDifficulty = difficultyController == null
            ? "NOT AVAILABLE"
            : difficultyController.SelectedDifficulty.ToString();

        if (Time.unscaledTime >= nextGameplayPollTime)
        {
            PollGameplayState();
            nextGameplayPollTime = Time.unscaledTime + GameplayPollInterval;
        }

        if (Time.unscaledTime >= nextMemorySampleTime)
        {
            SampleMemory();
            nextMemorySampleTime = Time.unscaledTime + MemorySampleInterval;
        }

        if (gameFlow.State == GameState.GameOver || gameFlow.State == GameState.Victory)
        {
            PollGameplayState();
            StopSession();
        }
    }

    private void ResolveReferences()
    {
        if (gameFlow == null)
        {
            gameFlow = FindFirstObjectByType<GameFlow>();
        }

        if (difficultyController == null)
        {
            difficultyController = FindFirstObjectByType<DifficultyController>();
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }
    }

    private void StartSession()
    {
        sessionActive = true;
        reportWritten = false;
        sessionStartTime = Time.unscaledTime;
        playingTime = 0f;
        frameTimeSum = 0f;
        minimumFrameTime = float.PositiveInfinity;
        maximumFrameTime = 0f;
        frameCount = 0;
        peakActiveEnemies = 0;
        totalEnemySpawns = 0;
        totalEnemyDeaths = 0;
        totalPoolGets = 0;
        totalPoolReturns = 0;
        previousActiveEnemyIds.Clear();
        currentActiveEnemyIds.Clear();
        startingManagedMemory = GC.GetTotalMemory(false);
        endingManagedMemory = startingManagedMemory;
        startingGcAllocatedBytes = GetGcAllocatedBytes();
        endingGcAllocatedBytes = startingGcAllocatedBytes;
        startingGcCollections = GetGcCollectionCount();
        endingGcCollections = startingGcCollections;
        SampleMemory();
        currentDifficulty = difficultyController == null
            ? "NOT AVAILABLE"
            : difficultyController.SelectedDifficulty.ToString();
        currentState = GameState.Playing.ToString();
    }

    private void StopSession()
    {
        if (!sessionActive || reportWritten)
        {
            return;
        }

        sessionActive = false;
        reportWritten = true;
        endingManagedMemory = GC.GetTotalMemory(false);
        endingGcAllocatedBytes = GetGcAllocatedBytes();
        endingGcCollections = GetGcCollectionCount();
        WriteReport();
    }

    private void RecordFrameSample()
    {
        if (!sessionActive)
        {
            return;
        }

        currentFrameTime = Time.unscaledDeltaTime;
        if (currentFrameTime <= 0f)
        {
            return;
        }

        frameCount++;
        frameTimeSum += currentFrameTime;
        minimumFrameTime = Mathf.Min(minimumFrameTime, currentFrameTime);
        maximumFrameTime = Mathf.Max(maximumFrameTime, currentFrameTime);
        currentFps = 1f / currentFrameTime;
    }

    private void PollGameplayState()
    {
        EnemyMovement[] activeEnemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        currentActiveEnemyIds.Clear();

        for (int index = 0; index < activeEnemies.Length; index++)
        {
            EnemyMovement enemy = activeEnemies[index];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            int instanceId = enemy.GetInstanceID();
            currentActiveEnemyIds.Add(instanceId);
            if (!previousActiveEnemyIds.Contains(instanceId))
            {
                totalEnemySpawns++;
                totalPoolGets++;
            }
        }

        foreach (int previousId in previousActiveEnemyIds)
        {
            if (!currentActiveEnemyIds.Contains(previousId))
            {
                totalPoolReturns++;
            }
        }

        lastActiveEnemyCount = currentActiveEnemyIds.Count;
        peakActiveEnemies = Mathf.Max(peakActiveEnemies, lastActiveEnemyCount);
        previousActiveEnemyIds.Clear();
        foreach (int currentId in currentActiveEnemyIds)
        {
            previousActiveEnemyIds.Add(currentId);
        }
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        if (sessionActive)
        {
            totalEnemyDeaths++;
        }
    }

    private void SampleMemory()
    {
        endingManagedMemory = GC.GetTotalMemory(false);
        endingGcAllocatedBytes = GetGcAllocatedBytes();
        endingGcCollections = GetGcCollectionCount();
        totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong();
        totalReservedMemory = Profiler.GetTotalReservedMemoryLong();
        graphicsMemory = Profiler.GetAllocatedMemoryForGraphicsDriver();
        displayedManagedMemory = endingManagedMemory / (1024f * 1024f);
        displayedGcAllocations = (endingGcAllocatedBytes - startingGcAllocatedBytes) / (1024f * 1024f);
    }

    private long GetGcAllocatedBytes()
    {
        try
        {
            return GC.GetAllocatedBytesForCurrentThread();
        }
        catch (NotSupportedException)
        {
            return -1;
        }
    }

    private int GetGcCollectionCount()
    {
        return GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2);
    }

    private void WriteReport()
    {
        string reportPath = Path.Combine(Application.persistentDataPath, ReportFileName);
        string report = BuildReport();
        try
        {
            File.WriteAllText(reportPath, report, Encoding.UTF8);
            Debug.Log("[RuntimePerformanceTelemetry] Report written to " + reportPath);
            AndroidReportExporter.Export(reportPath, ReportFileName);
        }
        catch (Exception exception)
        {
            Debug.LogError("[RuntimePerformanceTelemetry] Could not write report: " + exception.Message);
        }
    }

    private string BuildReport()
    {
        float duration = Mathf.Max(0f, Time.unscaledTime - sessionStartTime);
        float averageFrameTime = frameCount == 0 ? 0f : frameTimeSum / frameCount;
        float averageFps = duration <= 0f ? 0f : frameCount / duration;
        float minimumFps = maximumFrameTime <= 0f ? 0f : 1f / maximumFrameTime;
        float maximumFps = minimumFrameTime == float.PositiveInfinity ? 0f : 1f / minimumFrameTime;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("# Android Runtime Performance Baseline");
        builder.AppendLine();
        builder.AppendLine("Device: " + SystemInfo.deviceModel);
        builder.AppendLine("Android Version: " + SystemInfo.operatingSystem);
        builder.AppendLine("Build: " + (Debug.isDebugBuild ? "Development/Debug" : "Release"));
        builder.AppendLine("Unity Version: " + Application.unityVersion);
        builder.AppendLine();
        builder.AppendLine("Difficulty: " + currentDifficulty);
        builder.AppendLine("Test Duration: " + duration.ToString("F2") + " s");
        builder.AppendLine("Playing Duration: " + playingTime.ToString("F2") + " s");
        builder.AppendLine("Maximum Configured Enemies: " + (enemySpawner == null ? "NOT AVAILABLE" : enemySpawner.MaxActiveEnemies.ToString()));
        builder.AppendLine();
        builder.AppendLine("Average FPS: " + averageFps.ToString("F2"));
        builder.AppendLine("Minimum FPS: " + minimumFps.ToString("F2"));
        builder.AppendLine("Maximum FPS: " + maximumFps.ToString("F2"));
        builder.AppendLine("Average Frame Time: " + (averageFrameTime * 1000f).ToString("F2") + " ms");
        builder.AppendLine("Worst Frame Time: " + (maximumFrameTime * 1000f).ToString("F2") + " ms");
        builder.AppendLine();
        builder.AppendLine("Peak Active Enemies: " + peakActiveEnemies);
        builder.AppendLine("Total Enemy Spawns: " + totalEnemySpawns);
        builder.AppendLine("Total Enemy Deaths: " + totalEnemyDeaths);
        builder.AppendLine("Pool Gets: " + totalPoolGets);
        builder.AppendLine("Pool Returns: " + totalPoolReturns);
        builder.AppendLine();
        builder.AppendLine("GC Allocations: " + FormatMetric(displayedGcAllocations, " MB"));
        builder.AppendLine("GC Collections: " + (endingGcCollections - startingGcCollections));
        builder.AppendLine("Managed Memory: " + (displayedManagedMemory > 0f ? displayedManagedMemory.ToString("F2") + " MB" : "NOT AVAILABLE"));
        builder.AppendLine("Unity Allocated Memory: " + FormatBytes(totalAllocatedMemory));
        builder.AppendLine("Unity Reserved Memory: " + FormatBytes(totalReservedMemory));
        builder.AppendLine("Graphics Driver Memory: " + FormatBytes(graphicsMemory));
        builder.AppendLine();
        builder.AppendLine("Notes: Runtime-only telemetry; no Unity Profiler capture was used.");
        builder.AppendLine("Notes: GC allocation uses the current managed thread when supported by the runtime.");
        builder.AppendLine("Notes: Android device validation requires retrieving this file from persistentDataPath on the device.");
        return builder.ToString();
    }

    private string FormatMetric(float value, string suffix)
    {
        return value < 0f ? "NOT AVAILABLE" : value.ToString("F2") + suffix;
    }

    private string FormatBytes(long bytes)
    {
        return bytes <= 0 ? "NOT AVAILABLE" : (bytes / (1024f * 1024f)).ToString("F2") + " MB";
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private void OnGUI()
    {
        if (Time.unscaledTime < nextOverlayRefreshTime)
        {
            return;
        }

        nextOverlayRefreshTime = Time.unscaledTime + OverlayRefreshInterval;
        if (overlayStyle == null)
        {
            overlayStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                normal = { textColor = Color.white }
            };
        }

        GUILayout.BeginArea(new Rect(16f, 16f, 520f, 260f), GUI.skin.box);
        GUILayout.Label("PERFORMANCE TELEMETRY", overlayStyle);
        GUILayout.Label("State: " + currentState, overlayStyle);
        GUILayout.Label("Difficulty: " + currentDifficulty, overlayStyle);
        GUILayout.Label("FPS: " + (currentFps > 0f ? currentFps.ToString("F1") : "NOT AVAILABLE"), overlayStyle);
        GUILayout.Label("Frame: " + (currentFrameTime * 1000f).ToString("F2") + " ms", overlayStyle);
        GUILayout.Label("Enemies: " + lastActiveEnemyCount + " | Peak: " + peakActiveEnemies, overlayStyle);
        GUILayout.Label("Managed: " + (displayedManagedMemory > 0f ? displayedManagedMemory.ToString("F1") + " MB" : "N/A") + " | GC: " + FormatMetric(displayedGcAllocations, " MB"), overlayStyle);
        GUILayout.Label("Elapsed: " + (sessionActive ? (Time.unscaledTime - sessionStartTime).ToString("F1") : "0.0") + " s", overlayStyle);
        GUILayout.EndArea();
    }
#endif
}
