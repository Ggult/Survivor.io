# Performance Baseline

## Test Environment

- Unity version: 6000.0.70f1
- Build/Editor: Unity Editor Play Mode; Development Build and Autoconnect Profiler not used
- Platform: WindowsEditor
- Resolution: 1170x2532
- Quality settings: PC, quality level 0
- Graphics API: Direct3D11
- Rendering threading: MultiThreaded
- VSync: 0
- Target frame rate: -1
- Device: Editor host; Android device baseline: NOT VALIDATED
- Profiler: Unity Profiler enabled for runtime snapshots
- Important limitation: Time.frameCount and Time.renderedFrameCount remained at 2, so the gameplay/render loop did not advance during these MCP-driven samples. The values below are Editor snapshots, not a representative steady-state gameplay baseline.

## M5 Initial Attempt

- Time.frameCount remained approximately 2.
- CPU/GPU timeline and detailed Profiler metrics were not obtained.
- The observed 50 FPS / 20 ms values were not accepted as a steady-state baseline.

## M5 Profiling Attempt

- A previous large profiling capture produced approximately 500 MB of profiling data.
- Unity Editor became stuck while parsing profiling data and had to be force-closed.
- Large profiling capture, profiling-data parsing, and Profile Analyzer workflows are not considered reliable for this project baseline.

## M5.1 Safe Runtime Baseline

### Runtime Validation

- MCP/Unity access: PASS for the safe state calls used in this attempt.
- Play Mode: PASS; Unity entered Play Mode.
- frameCount: 2 in both safe snapshots; minimum `> 300`: NOT VALIDATED.
- Time.time: 0.02 in both safe snapshots; progression: NOT VALIDATED.
- GameFlow state: initial snapshot `Victory`; after explicit Hard selection `Playing`.
- Difficulty: initial `Medium`; Hard scenario explicitly selected and observed.
- Timer: initial `0.00`; after Hard start `180.00`; progression: NOT VALIDATED.
- Active enemies: 0 in the Hard snapshot; minimum `> 0`: NOT VALIDATED.
- Spawn status: NOT VALIDATED; no enemy spawn was observed.
- Pool status: NOT VALIDATED.
- Hard configured maximum: 25; maximum observed active enemies: NOT VALIDATED.

### Easy

- Runtime validation: NOT VALIDATED.
- frameCount, timer progression, enemy spawn, and active enemy count: NOT VALIDATED.
- FPS baseline: NOT AVAILABLE; no reliable runtime window was observed.
- CPU/GPU/GC/Rendering/Memory: NOT AVAILABLE; no large profiling capture used.

### Medium

- Runtime validation: NOT VALIDATED.
- frameCount, timer progression, enemy spawn, and active enemy count: NOT VALIDATED.
- FPS baseline: NOT AVAILABLE; no reliable runtime window was observed.
- CPU/GPU/GC/Rendering/Memory: NOT AVAILABLE; no large profiling capture used.

### Hard

- Runtime validation: NOT VALIDATED.
- Hard state and configuration were selected successfully: `Playing`, max enemies `25`, spawn interval `0.70 s`.
- frameCount: 2; Time.time: 0.02; Timer: 180.00.
- Active enemies: 0; enemy spawn and steady-state behavior: NOT VALIDATED.
- FPS baseline: NOT AVAILABLE; no reliable runtime window was observed.
- CPU/GPU/GC/Rendering/Memory: NOT AVAILABLE; no large profiling capture used.

## Measurement Limitations

- The MCP-driven Editor runtime did not advance beyond frame 2 during the safe snapshots.
- A reliable CPU/GPU/GC/Rendering profiler capture was intentionally not taken because the previous approximately 500 MB capture locked Unity during parsing.
- FPS and frame-time averages, minimums, and maximums were not calculated from the non-advancing runtime.
- Android device baseline: NOT VALIDATED.
- Unity MCP workflow üzerinden güvenilir CPU/GPU/GC/Rendering profiler capture alınamadığı için bu metrikler ölçülmedi.

## Baseline Conclusion

- Baseline NOT VALIDATED.
- The safe attempt confirmed that Hard can be selected, but it did not confirm runtime progression, timer progression, enemy spawning, or steady-state active enemy load.

## M5.2 Safe Runtime Recheck

### Root Cause Found

- Unity Editor and MCP were responsive; the Unity process was not crashed.
- `Application.isPlaying` was true and `Time.timeScale` was 1, but `Application.runInBackground` was false.
- Because the Editor/Game View was not the active foreground loop, the MCP-triggered Play Mode remained at `frameCount=2` and `Time.time=0.02`.
- `Application.runInBackground=true` was enabled temporarily at runtime only. It was not written to Project Settings and no gameplay code was changed.

### Runtime Validation

- Easy: `frameCount=2387`, `Time.time=24.25`, `GameFlow=Playing`, timer remaining `174.77`, active enemies `2`, spawn interval `2.00 s`, configured maximum `8`.
- Medium: `frameCount=5461`, `Time.time=55.64`, `GameFlow=GameOver`, timer remaining `166.09`, active enemies `10`, spawn interval `1.20 s`, configured maximum `15`.
- Hard: `frameCount=1107`, `Time.time=11.39`, `GameFlow=GameOver`, timer remaining `170.62`, active enemies `13`, spawn interval `0.70 s`, configured maximum `25`.
- Minimum runtime validation: PASS. All scenarios exceeded `frameCount > 300`; timer values decreased from the `180.00` start; enemy spawning and pool retrieval were observed.
- Hard maximum enemy count: NOT VALIDATED. The run reached 13 active enemies before the player died; 25 was not observed.
- Pool activity: PASS for observed `EnemyPool.Get` and `EnemyPool.Return` logs.
- Gameplay activity: PASS for observed enemy attack, player attack, damage, spawn, and pool logs.

### Safe Metrics

- FPS baseline: NOT AVAILABLE as a separately sampled average/min/max window. No reliable FPS claim is made from these snapshots.
- CPU/GPU/GC/Rendering/Memory: NOT AVAILABLE. No large profiling capture or parsing workflow was used.
- Android device baseline: NOT VALIDATED.

## Updated Baseline Conclusion

- PARTIAL BASELINE - runtime validated, detailed profiler metrics unavailable.
- FULL PERFORMANCE BASELINE NOT VALIDATED.
- The next measurement can use the same temporary runtime-only background execution condition, but should still avoid large Profiler captures unless a small, bounded capture workflow is independently verified.

## M5.3 Android Runtime Performance Telemetry

- Added isolated `RuntimePerformanceTelemetry` runtime telemetry with no gameplay or UI layout changes.
- The telemetry records frame timing/FPS, active and peak enemies, inferred spawns and pool transitions, enemy deaths, managed memory, GC allocations/collections, and Unity memory counters.
- A development/editor overlay is available during a run. The final report is written at GameOver or Victory to `Application.persistentDataPath/Performance_Android_Baseline.md`.
- No Unity Profiler capture, Profile Analyzer session, or large profiling data capture was used.
- Editor smoke test: PASS. The telemetry object bootstrapped in Play Mode and no Unity Error logs were reported.
- Android device baseline: NOT VALIDATED. The generated device report must be copied to `Documentation/Performance_Android_Baseline.md` after a Development Build run.

## Easy

- Active enemies: 0 at sample time; steady-state value NOT VALIDATED
- Configured maximum enemies: 8
- Configured spawn interval: 2.00 s
- FPS: 50.000 snapshot; NOT VALIDATED as stable gameplay FPS
- Frame time: 20.000 ms snapshot; NOT VALIDATED as stable gameplay frame time
- CPU: NOT AVAILABLE from the exposed snapshot API
- GPU: NOT AVAILABLE from the exposed snapshot API
- GC Alloc: NOT AVAILABLE
- GC Collections: NOT AVAILABLE
- Batches: NOT AVAILABLE
- SetPass: NOT AVAILABLE
- Triangles: NOT AVAILABLE
- Vertices: NOT AVAILABLE
- Shadow workload: NOT AVAILABLE
- SkinnedMeshRenderer workload: NOT AVAILABLE
- Scripts/Physics/Animation: NOT AVAILABLE as timing breakdowns
- Memory snapshot: Total reserved 957.46 MB; total allocated 532.49 MB; graphics 289.97 MB; mono used 817.27 MB; managed GC memory 807.20 MB
- Spawn frequency: NOT VALIDATED because the runtime frame loop did not advance
- Pool Get/Return activity: NOT VALIDATED

## Medium

- Active enemies: 0 at sample time; steady-state value NOT VALIDATED
- Configured maximum enemies: 15
- Configured spawn interval: 1.20 s
- FPS: 50.000 snapshot; NOT VALIDATED as stable gameplay FPS
- Frame time: 20.000 ms snapshot; NOT VALIDATED as stable gameplay frame time
- CPU: NOT AVAILABLE from the exposed snapshot API
- GPU: NOT AVAILABLE from the exposed snapshot API
- GC Alloc: NOT AVAILABLE
- GC Collections: NOT AVAILABLE
- Batches: NOT AVAILABLE
- SetPass: NOT AVAILABLE
- Triangles: NOT AVAILABLE
- Vertices: NOT AVAILABLE
- Shadow workload: NOT AVAILABLE
- SkinnedMeshRenderer workload: NOT AVAILABLE
- Scripts/Physics/Animation: NOT AVAILABLE as timing breakdowns
- Memory snapshot: Total reserved 1872.54 MB; total allocated 1406.27 MB; graphics 302.02 MB; mono used 856.05 MB; managed GC memory 856.05 MB
- Spawn frequency: NOT VALIDATED because the runtime frame loop did not advance
- Pool Get/Return activity: NOT VALIDATED

## Hard

- Active enemies: 0 at sample time; steady-state value NOT VALIDATED
- Configured maximum enemies: 25
- Configured spawn interval: 0.70 s
- FPS: 50.000 snapshot; NOT VALIDATED as stable gameplay FPS
- Frame time: 20.000 ms snapshot; NOT VALIDATED as stable gameplay frame time
- CPU: NOT AVAILABLE from the exposed snapshot API
- GPU: NOT AVAILABLE from the exposed snapshot API
- GC Alloc: NOT AVAILABLE
- GC Collections: NOT AVAILABLE
- Batches: NOT AVAILABLE
- SetPass: NOT AVAILABLE
- Triangles: NOT AVAILABLE
- Vertices: NOT AVAILABLE
- Shadow workload: NOT AVAILABLE
- SkinnedMeshRenderer workload: NOT AVAILABLE
- Scripts/Physics/Animation: NOT AVAILABLE as timing breakdowns
- Memory snapshot: Total reserved 2385.54 MB; total allocated 1922.72 MB; graphics 302.04 MB; mono used 897.08 MB; managed GC memory 897.07 MB
- Spawn frequency: NOT VALIDATED because the runtime frame loop did not advance
- Pool Get/Return activity: NOT VALIDATED
- Maximum observed enemies: 0 in the non-advancing Editor sample; maximum configured enemies 25

## Initial Bottleneck Candidates

- No CPU bottleneck candidate is supported by these measurements. CPU timeline and category breakdowns were not available.
- No GPU bottleneck candidate is supported by these measurements. GPU timing and rendering counters were not available.
- Enemy movement, physics queries, attack, Animator, rendering, shadows, and pooling cannot be ranked from this run because no gameplay frames advanced and no detailed Profiler timeline was captured.
- The rising memory snapshots across difficulty changes are not a bottleneck finding. They were captured in one Editor session and are not isolated per-scenario measurements.

## Optimization Plan

1. Repeat Easy, Medium, and Hard in a real runtime loop with a fixed warm-up and steady-state capture window.
2. Capture CPU Usage, Rendering, Physics, Animation, and Memory timeline data with the Unity Profiler or Profile Analyzer.
3. Record GC.Alloc and GC collection counts over the same capture window.
4. Capture batches, SetPass calls, triangles, vertices, shadows, and skinned mesh renderer work from the Rendering module.
5. Repeat the same scenarios in a Development Build on the target Android device before selecting an optimization target.

## Validation Summary

- Easy scenario executed: PASS, but representative baseline: NOT VALIDATED
- Medium scenario executed: PASS, but representative baseline: NOT VALIDATED
- Hard scenario executed: PASS, but maximum-enemy stabilization: NOT VALIDATED
- Android device baseline: NOT VALIDATED
- Gameplay code changed: NO
- Optimization or refactor performed: NO
- Difficulty, pooling, rendering, URP, assets, UI, and gameplay settings changed: NO
- Console errors/exceptions during measurement: none observed in the measurement window
