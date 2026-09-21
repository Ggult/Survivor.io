# M7 - Mobile Performance Optimization Analysis

## Scope

This is an inspection-only report. No gameplay code, assets, URP settings, project settings, difficulty values, pooling behavior, UI, camera, arena, or Muzzle Flash changes were made.

The analysis uses the M6 lightweight runtime telemetry reports from a real Android device:

- Device: Huawei COR-L29
- Android: Android 9 / API 28
- Unity: 6000.0.70f1
- Build: Release
- Measurement method: project runtime telemetry; no Unity Profiler capture or Profile Analyzer session

## Baseline

| Difficulty | Average FPS | Average Frame Time | Minimum FPS | Worst Frame Time | Peak Enemies | Unity Allocated Memory | GC Allocations |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Easy | 29.94 | 33.40 ms | 8.57 | 116.63 ms | 9 | 124.37 MB | 0 MB |
| Medium | 29.59 | 33.80 ms | 12.00 | 83.31 ms | 16 | 124.78 MB | 0 MB |
| Hard | 29.00 | 34.48 ms | 12.00 | 83.31 ms | 26 | 125.71 MB | 0 MB |

The baseline shows a real device frame-rate ceiling near 30 FPS and occasional long frames. It does not identify whether the long frames are CPU, GPU, thermal, OS scheduling, or device refresh behavior because no detailed timeline was captured.

## Investigation A - Approximately 30 FPS Ceiling

### Finding

**Not fully explained. The ceiling is not supported as an intentional project frame-rate cap.**

Evidence inspected:

- No `Application.targetFrameRate` assignment was found in the project C# scripts.
- Mobile quality has `vSyncCount: 0` in `ProjectSettings/QualitySettings.asset`.
- Android uses the Mobile quality profile.
- `ProjectSettings/ProjectSettings.asset` does not define a target frame rate value for this project.
- The Android device measured approximately 29-30 FPS across all difficulties, but that alone cannot distinguish device refresh behavior from a runtime performance ceiling.
- The result is consistent with a device/display/runtime cap, but this remains a hypothesis without a controlled target-frame experiment or device refresh-rate measurement.

No target frame rate was changed. No conclusion is made that the application is CPU-bound or GPU-bound.

## Investigation B - Enemy Count +1

### Finding

**Explained as a metric-definition/lifecycle timing mismatch, not evidence that the spawner exceeds its configured limit.**

Evidence:

- `EnemySpawner.TrySpawnEnemy` checks `activeEnemies.Count >= maxActiveEnemies` before spawning and adds the enemy to its active list after `EnemyPool.Get`.
- `EnemySpawner.CleanupInactiveEnemies` removes enemies when `EnemyHealth.IsDead` is true or the GameObject is inactive.
- `EnemyHealth.BeginDeath` sets `isDead = true` and disables movement, attack, and the separation collider, but the GameObject remains active during the death animation and is returned to the pool only after the death delay.
- `RuntimePerformanceTelemetry.PollGameplayState` counts active `EnemyMovement` GameObjects using `FindObjectsByType` and `activeInHierarchy`; it does not exclude `EnemyHealth.IsDead` enemies.
- Therefore, an enemy in its death-animation window can be included by telemetry while already excluded from the spawner's capacity list.

This explains the observed values: configured 8/15/25 versus telemetry peaks 9/16/26. The spawner limit was not changed.

## Optimization Candidates

### HIGH - None justified

No candidate is classified HIGH. The available baseline has no CPU/GPU breakdown, and the memory and GC results do not show a high-confidence allocation or memory bottleneck.

### MEDIUM - Cache `EnemyAttack` and `EnemyHealth` references in `EnemySpawner`

- **Problem:** `EnemySpawner` calls `GetComponent<EnemyAttack>()` twice for every spawn and calls `GetComponent<EnemyHealth>()` during inactive cleanup.
- **Evidence:** `Assets/Scripts/Enemy/EnemySpawner.cs` lines 113-114 and 123.
- **Expected impact:** Possible small CPU reduction during spawning and cleanup, especially as enemy count rises. Spawn/cleanup are low-frequency relative to per-frame movement.
- **Risk:** Low code risk, but pooled component reference ownership must remain correct.
- **Proposed change:** Store required component references in a small per-enemy cached structure or expose cached references from the enemy component. Do not change spawning rules.
- **Validation:** Repeat identical Easy/Medium/Hard Android runs and compare average/worst frame time, peak enemies, spawn counts, and pool counts.
- **Classification:** MEDIUM code-level optimization candidate; not a measured bottleneck.

### MEDIUM - Remove per-frame fallback lookup in `EnemyAttack.Update`

- **Problem:** `EnemyAttack.Update` calls `GetComponent<Animator>()` whenever its serialized animator reference is null.
- **Evidence:** `Assets/Scripts/Enemy/EnemyAttack.cs` lines 33-38. The lookup is currently a fallback, so its cost depends on prefab wiring.
- **Expected impact:** Small CPU reduction if the reference is missing on active pooled enemies.
- **Risk:** Low, provided the existing `Awake` cache and prefab references remain functionally equivalent.
- **Proposed change:** Verify prefab wiring and make the cached reference invariant. Do not alter attack timing or animation behavior.
- **Validation:** Confirm animator references on the Enemy prefab, then repeat the same device test and compare telemetry. A code change should only be made if the missing-reference path is observed.
- **Classification:** MEDIUM conditional candidate; not justified as an unconditional bottleneck.

### MEDIUM - Reduce runtime diagnostic logging in release performance runs

- **Problem:** Combat, damage, death, and pool lifecycle paths emit formatted `Debug.Log` messages during gameplay.
- **Evidence:** `Assets/Scripts/Player/PlayerAutoAttack.cs` line 62, `Assets/Scripts/Enemy/EnemyAttack.cs` line 62, `Assets/Scripts/Enemy/EnemyHealth.cs` lines 68 and 115, and `Assets/Scripts/Enemy/EnemyPool.cs` lines 46 and 59.
- **Expected impact:** Possible CPU, string-formatting, log-buffer, and I/O reduction, particularly during high enemy activity. The M6 GC telemetry reported 0 MB, so allocation impact is not demonstrated.
- **Risk:** Removing logs can reduce debugging visibility and must not change gameplay behavior.
- **Proposed change:** Guard diagnostic logs behind an explicit development-only or telemetry logging condition after baseline comparison.
- **Validation:** Compare release Android builds with equivalent scenarios, using the same report duration and device. Verify no gameplay counters change.
- **Classification:** MEDIUM hypothesis; not a measured bottleneck.

### LOW - Reduce telemetry polling cost during production measurement

- **Problem:** `RuntimePerformanceTelemetry.PollGameplayState` calls `FindObjectsByType<EnemyMovement>` and rebuilds HashSet state every 0.25 seconds.
- **Evidence:** `Assets/Scripts/Performance/RuntimePerformanceTelemetry.cs` lines 108, 120, and 208-233.
- **Expected impact:** Small reduction in measurement overhead and temporary array/search work. It does not optimize gameplay when telemetry is removed or disabled.
- **Risk:** Changing polling can alter the comparability of the M6 measurement method and make pool counts less representative.
- **Proposed change:** Do not change during the first optimization pass. If needed later, separate measurement mode from production mode or observe lifecycle events without changing gameplay systems.
- **Validation:** Compare telemetry-enabled and telemetry-disabled builds; preserve the same sampling semantics before using the report for a new baseline.
- **Classification:** LOW and currently not justified for gameplay optimization.

### LOW - Reduce repeated separation physics queries only after profiling evidence

- **Problem:** Every active enemy performs `Physics.OverlapSphereNonAlloc` for separation every frame.
- **Evidence:** `Assets/Scripts/Enemy/EnemyMovement.cs` lines 24-52; M6 Hard reached a telemetry peak of 26 enemies.
- **Expected impact:** Potential CPU reduction as enemy population increases. The query uses a non-allocating buffer, so the allocation risk is limited.
- **Risk:** High gameplay-feel risk if update frequency, radius, or separation behavior changes. No CPU timeline proves this is expensive on the target device.
- **Proposed change:** No immediate change. First obtain a bounded CPU measurement or add a narrowly scoped timing counter that does not alter behavior.
- **Validation:** Compare movement/separation timing and gameplay behavior at the same enemy loads on the Huawei device.
- **Classification:** LOW candidate / optimization hypothesis, not a justified change yet.

### LOW - Rendering and post-processing review

- **Problem:** The scene camera has HDR, post-processing enabled, and a directional light with shadows. The SampleScene volume profile enables Bloom, Vignette, and Tonemapping; Mobile URP has main-light shadows and HDR support enabled.
- **Evidence:** `Assets/Scenes/SampleScene.unity` camera and light settings; `Assets/Settings/SampleSceneProfile.asset`; `Assets/Settings/Mobile_RPAsset.asset`.
- **Expected impact:** Potential GPU reduction if these features are expensive on the target device.
- **Risk:** Visible quality regression. The M6 data contains no GPU timing, draw-call, overdraw, or thermal evidence.
- **Proposed change:** No immediate rendering setting change. Test isolated feature variants only in a separate measurement pass if a GPU-oriented metric becomes available.
- **Validation:** Same device, same scene, same run duration, and controlled A/B build variants with visual checks.
- **Classification:** LOW hypothesis; not justified by current measurements.

### LOW - Asset and animation audit

- **Problem:** Player and enemy use imported FBX models, skinned animation, and multiple texture maps. The source `enemy.fbx` is approximately 98 MB on disk; the source `player.fbx` is approximately 5.5 MB.
- **Evidence:** `Assets/case_models/` source assets and Player/Enemy prefabs. Enemy and Player Animator components use normal update mode and are not configured for physics animation.
- **Expected impact:** Possible build size, load-time, memory, or skinning improvements after inspecting imported mesh/texture data.
- **Risk:** Visual or animation regression; source assets are explicitly protected from replacement.
- **Proposed change:** No asset replacement or import-setting change in this pass. The M6 allocated-memory values are close across difficulties, so no asset memory bottleneck is established.
- **Validation:** Inspect imported mesh vertex/bone counts and texture dimensions, then compare Android memory and frame timing with controlled import-setting variants.
- **Classification:** LOW hypothesis; not justified by the current telemetry alone.

### NOT JUSTIFIED - Change difficulty limits, spawn intervals, pooling, or gameplay behavior

- **Evidence:** The M6 measurements intentionally compare Easy/Medium/Hard load differences. The enemy +1 is explained by active death-animation objects being counted by telemetry.
- **Expected impact:** Could raise FPS by reducing the workload, but would invalidate the gameplay and baseline comparison rather than optimize implementation.
- **Risk:** Direct gameplay and balancing regression.
- **Proposed change:** None.
- **Validation:** Not applicable; these are explicitly excluded from M7.
- **Classification:** NOT JUSTIFIED.

## Inspected Files and Areas

### Code

- `Assets/Scripts/Player/PlayerMovement.cs`
- `Assets/Scripts/Player/PlayerRotation.cs`
- `Assets/Scripts/Player/PlayerAutoAttack.cs`
- `Assets/Scripts/Enemy/EnemyMovement.cs`
- `Assets/Scripts/Enemy/EnemyAttack.cs`
- `Assets/Scripts/Enemy/EnemyHealth.cs`
- `Assets/Scripts/Enemy/EnemySpawner.cs`
- `Assets/Scripts/Enemy/EnemyPool.cs`
- `Assets/Scripts/GameFlow.cs`
- `Assets/Scripts/DifficultyController.cs`
- `Assets/Scripts/Performance/RuntimePerformanceTelemetry.cs`
- `Assets/Scripts/Performance/AndroidReportExporter.cs`
- `Assets/Scripts/Player/PlayerAnimation.cs`
- `Assets/Scripts/Player/PlayerRotationDecision.cs`
- `Assets/Scripts/Player/PlayerCameraFollow.cs`
- `Assets/Scripts/Player/MovementInputConsumer.cs`

### Rendering, animation, and scene

- `Assets/Prefabs/Player.prefab`
- `Assets/Prefabs/Enemy.prefab`
- `Assets/Prefabs/Rifle.prefab`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Settings/Mobile_RPAsset.asset`
- `Assets/Settings/Mobile_Renderer.asset`
- `Assets/Settings/PC_RPAsset.asset`
- `Assets/Settings/PC_Renderer.asset`
- `Assets/Settings/SampleSceneProfile.asset`
- `Assets/Settings/DefaultVolumeProfile.asset`
- `ProjectSettings/QualitySettings.asset`
- `ProjectSettings/ProjectSettings.asset`

### Assets

- Provided Player, Enemy, Rifle FBX files
- Provided Player and Enemy texture maps
- Provided Player and Enemy animation FBX files
- `Assets/Materials/HitParticle.mat`
- `Assets/Textures/New Material.mat`

## M7 Decision

- Optimization changes made: **NO**
- Gameplay behavior changed: **NO**
- Assets changed: **NO**
- URP or project settings changed: **NO**
- ~30 FPS behavior: **Not fully explained; no project target/VSync cap found, device/runtime cap remains a hypothesis.**
- Enemy +1 behavior: **Explained as telemetry counting active death-animation objects while the spawner excludes dead enemies from its capacity list.**
- Performance improvement claimed: **NO**

## M7.2 - Applied Mobile Optimizations

### Scope and measurement separation

The M6 Android baseline remains the comparison baseline. No Android performance measurement, large Unity Profiler capture, or final FPS claim was made during M7.2. `RuntimePerformanceTelemetry.cs` was intentionally left unchanged so the final Android measurement remains comparable.

The mobile runtime now explicitly targets 60 FPS with vSync disabled. This is a frame-rate request, not a measured guarantee; the final Android benchmark will determine whether the device sustains it under enemy load.

### Applied optimizations

#### Enemy separation query throttling

- **Problem:** Every moving enemy performed `Physics.OverlapSphereNonAlloc` every frame.
- **Existing implementation:** The query used a reusable buffer, but still consumed physics CPU for every active enemy on every frame.
- **Change:** `EnemyMovement` now refreshes the separation direction every 0.08 seconds (approximately 12.5 Hz), retains the last result between checks, and keeps movement and rotation per-frame. Target changes force an immediate refresh.
- **Expected benefit:** Reduced repeated physics-query CPU work as enemy count rises, with no managed allocation increase.
- **Risk:** Separation response can be slightly less immediate between samples; radius, strength, stopping distance, speed, and basic movement are unchanged.
- **Validation:** `EnemyMovement.cs` compiled with no diagnostics; prefab and scene smoke validation remains required before the final Android benchmark.

#### Enemy renderer feature removal

- **Problem:** The single Enemy SkinnedMeshRenderer was casting and receiving shadows, using blended probes, and generating object motion vectors.
- **Existing implementation:** Audit found shadows `On`, receive shadows enabled, blended light/reflection probes, and object motion vectors.
- **Change:** Enemy renderer now uses shadow casting `Off`, receive shadows disabled, light/reflection probes disabled, and `ForceNoMotion`.
- **Expected benefit:** Lower per-enemy shadow-map, probe, and motion-vector rendering work and GPU bandwidth.
- **Risk:** Enemies no longer contribute individual shadows or probe-based lighting; gameplay readability does not depend on enemy shadows.
- **Validation:** Unity readback confirmed the requested renderer values; mesh, bones, materials, and animation clips were unchanged.

#### Enemy Animator culling

- **Problem:** Enemy Animator was configured as `AlwaysAnimate`.
- **Change:** Enemy prefab Animator culling mode is now `CullUpdateTransforms`. Script-driven movement, attack timing, controller, parameters, update mode, and root motion behavior remain unchanged.
- **Expected benefit:** Avoids unnecessary off-screen animation transform updates while retaining state evaluation.
- **Risk:** Off-screen bone transforms are not updated until visible again; enemy movement is script-driven and root motion is disabled.
- **Validation:** Unity readback confirmed `CullUpdateTransforms`.

#### Enemy material instancing

- **Problem:** Repeated enemies use the same two shared materials, but instancing was not enabled.
- **Change:** Enabled `Material.enableInstancing` on both existing shared Enemy materials without creating runtime material instances or replacing shaders.
- **Expected benefit:** Allows compatible Enemy draws to use GPU instancing where supported, alongside SRP Batcher compatibility.
- **Risk:** No material properties or appearance were changed.
- **Validation:** Unity readback confirmed `instancing=True,True` and two shared materials remain assigned.

#### Mobile frame pacing and quality reductions

- **Problem:** No runtime `Application.targetFrameRate` was configured, and the Mobile profile retained avoidable lighting, filtering, and particle budgets.
- **Existing implementation:** vSync was already 0, but target FPS was unset. Mobile quality used two pixel lights, medium shadows at 40 metres, two cascades, forced anisotropic filtering, realtime GI CPU usage 100, and particle raycast budget 256. Mobile URP used 1024 main shadow resolution, shadow distance 50, and additional lights enabled.
- **Change:** Added `MobilePerformanceSettings` with `Application.targetFrameRate=60` and `QualitySettings.vSyncCount=0`. Mobile quality now uses zero pixel lights, low shadow resolution, one cascade, 20 metre shadow distance, disabled anisotropic filtering, zero realtime GI CPU usage, and particle raycast budget 64. Mobile URP main shadow resolution is 512, shadow distance 20, and additional lights are disabled.
- **Expected benefit:** Lower global GPU/CPU lighting, shadow, texture-filtering, and particle physics cost while allowing the device to target 60 FPS.
- **Risk:** Lower scene lighting/shadow quality, reduced texture anisotropy, and less particle collision precision. These are deliberate mobile visual concessions requested for this pass.
- **Validation:** Runtime readback confirmed target FPS 60, vSync 0, shadow distance 20, Low shadow resolution, one cascade, zero pixel lights, disabled anisotropic filtering, and particle budget 64.

#### Camera and Mobile URP GPU reductions

- **Problem:** The active scene camera still allowed HDR, camera MSAA, URP post-processing, and camera shadow rendering even though the Mobile URP asset already disabled MSAA and the Global Volume weight was zero.
- **Change:** Main Camera HDR and MSAA permission are disabled. `UniversalAdditionalCameraData` now disables post-processing and shadow rendering. Mobile URP main-light shadow support is disabled as a pipeline-level safeguard.
- **Expected benefit:** Removes HDR buffer bandwidth, unnecessary post-processing checks, camera shadow work, and mobile main-light shadow variants from the active rendering path.
- **Risk:** Lower scene contrast/effects and no camera-rendered shadows. The scene's Global Volume already had weight 0 and the Directional Light already had shadow type None, so no active gameplay lighting dependency was removed.
- **Validation:** Unity readback confirmed camera HDR/MSAA/post-processing/shadow flags are disabled; the Mobile URP asset reports main-light shadows unsupported.

#### Skinned mesh triangle reduction

- **Problem:** The supplied skinned models carried more geometry than needed for the requested mobile visual-quality tradeoff: Enemy `36,902` tris and Player `19,450` tris total.
- **Change:** Blender-generated optimized FBX copies were created without modifying the original source FBX files. Unity mesh assets were then generated from those copies, with bone weights and bind poses remapped to the existing prefab bone order before assignment.
- **Result:** Enemy `36,902 -> 18,450` tris (`50.0%` reduction). Player `19,450 -> 9,724` tris (`50.01%` reduction).
- **Preserved:** Enemy 52-bone prefab binding, Player 69-bone prefab bindings, existing materials, Animator/controller references, prefab hierarchy, colliders, and gameplay scripts.
- **Risk:** Silhouette and deformation quality may degrade, especially at close range. The original FBX files remain available for rollback.
- **Validation:** Unity readback confirmed optimized mesh references, matching bone and bind-pose counts, and preserved material counts. Final visual animation smoke and Android FPS measurement remain required.

The already accepted M7.1 changes remain active:

- `EnemySpawner` caches `EnemyAttack` and `EnemyHealth` references after pooling.
- `EnemyAttack` no longer performs a per-frame fallback `GetComponent<Animator>` lookup; the `Enemy.prefab` reference is serialized to its existing Animator.
- Combat, damage, death, and pool diagnostic logs are development/editor-only.

These changes preserve gameplay ownership and do not change spawn interval, enemy limits, attack timing/damage, death timing, pooling semantics, or targeting behavior. Their expected benefit is reduced avoidable CPU/logging work; final impact will be measured only in the final Android validation.

### Rejected / Not Applied

#### Enemy separation

`EnemyMovement` still uses `Physics.OverlapSphereNonAlloc` with a reusable 32-entry buffer. The query frequency was reduced, but its radius, strength, layer mask, buffer size, and movement behavior were intentionally preserved. The M6 Hard telemetry peak of 26 is explained by death-animation lifecycle counting rather than a spawner limit change.

#### Player target scanning

`PlayerAutoAttack` uses `OverlapSphereNonAlloc` and a reusable 64-entry buffer. Changing scan cadence or targeting order would touch explicitly protected targeting behavior, so it was left unchanged.

#### URP and rendering settings

The mobile URP asset has opaque and depth textures disabled, MSAA disabled, no renderer features, no additional light shadows, no main-light shadows, and a render scale of 0.8. Main Camera HDR, camera MSAA, post-processing, and camera shadow rendering are also disabled. The Global Volume remains in the scene with weight 0, and the Directional Light already had shadow type None.

#### Assets, textures, meshes, and animation imports

The supplied asset tree is approximately 219 MB and includes the approximately 98 MB `enemy.fbx`, separate animation FBX files, textures, and source material data. The original supplied assets were not overwritten. No imported mesh, texture, FBX animation, humanoid retargeting, Animator controller, skin quality, or existing animation clip was changed because compatibility risk is material and no safe reduced variant was available.

#### Runtime telemetry

Telemetry polling uses `FindObjectsByType` every 0.25 seconds and is a known measurement overhead. It was not changed because it is required for the final measurement and changing sampling semantics would weaken comparison with M6.

#### Gameplay and project configuration

Difficulty values, spawn interval, maximum enemies, pooling, player behavior, enemy attack/death behavior, UI, camera, arena, Muzzle Flash, and GameFlow were not changed. Texture import settings, physics collision matrix, package, architecture layer, ECS/DOTS system, NavMesh system, and profiler capture were not changed.

### Validation

- Unity AssetDatabase refresh completed successfully after the M7.1 state was inspected.
- `EnemyMovement.cs` reported no editor diagnostics after the separation change.
- Enemy prefab smoke validation confirmed the Animator reference, EnemyMovement, EnemyHealth, and EnemyAttack components are present and correctly wired.
- Unity audit and post-write readback confirmed the Enemy renderer and Animator optimization values.
- Both shared Enemy materials report GPU instancing enabled.
- No Unity test assembly is present, so automated EditMode tests were unavailable (`No tests found`).
- Play Mode end-to-end validation and the final Android measurement remain pending and must happen after project completion.

### M7.2 result

Gameplay behavior changed: **NO**

New runtime/rendering/quality optimizations applied: **YES**

Rendering and quality settings changed: **YES** (Enemy prefab renderer, Mobile profile, Mobile URP, Main Camera, and skinned mesh assets)

Performance improvement claimed: **NO**

Remaining possible bottlenecks are global lighting/post-processing cost, texture/import footprint, and residual skinning/material cost. Final performance impact will be measured in the final Android benchmark. The exact next step is: **FINAL ANDROID MEASUREMENT AFTER PROJECT COMPLETION**.
