# Android Runtime Performance Baseline Archive

The generated Android reports are preserved as immutable before/after artifacts under `Assets/PerformanceAnalysis`. This file indexes the measurement procedure and the final comparison; the individual files contain the raw device values.

The runtime report is written to `Application.persistentDataPath/Performance_Android_Baseline.md` when a run reaches GameOver or Victory. Copy that file into this location after testing.

## Test Procedure

1. Build and install an Android Development Build.
2. Launch the game and select a difficulty.
3. Keep the telemetry overlay visible during normal gameplay.
4. Play until GameOver or Victory so the report is generated.
5. Retrieve `Performance_Android_Baseline.md` from the device's `persistentDataPath`.
6. Copy the generated report here without changing its measured values.

## Final reports

Device: HUAWEI COR-L29, Android OS 9 / API-28
Build: Release
Unity Version: 6000.0.70f1

| Difficulty | Before average FPS | After average FPS | Before worst frame | After worst frame |
| --- | ---: | ---: | ---: | ---: |
| Easy | 29.94 | 50.05 | 116.63 ms | 116.63 ms |
| Medium | 29.59 | 60.02 | 83.31 ms | 16.66 ms |
| Hard | 29.00 | 59.02 | 83.31 ms | 33.33 ms |

Before reports:

- `Assets/PerformanceAnalysis/BeforeOptimization/Performance_Android_Baseline_Easy.md`
- `Assets/PerformanceAnalysis/BeforeOptimization/Performance_Android_Baseline_Medium.md`
- `Assets/PerformanceAnalysis/BeforeOptimization/Performance_Android_Baseline_Hard.md`

After reports:

- `Assets/PerformanceAnalysis/AfterOptimization/Performance_Android_Baseline_Easy.md`
- `Assets/PerformanceAnalysis/AfterOptimization/Performance_Android_Baseline_Medium.md`
- `Assets/PerformanceAnalysis/AfterOptimization/Performance_Android_Baseline_Hard.md`

All reports use runtime-only telemetry. No Unity Profiler capture was used, and graphics-driver memory was unavailable on this device.

## Validation Status

Android before/after runtime comparison: VALIDATED
Detailed CPU/GPU timeline: NOT AVAILABLE
Automated Unity tests: NOT AVAILABLE; manual Unity/MCP smoke validation was used
