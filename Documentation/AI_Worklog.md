# AI Worklog

## M7.2 - Final Optimization Pass

### Scope

M7.2 was an evidence-based final optimization inspection after M7.1. The M6 Android measurement remains the baseline. No Android benchmark, final FPS claim, or large Unity Profiler capture was performed. `RuntimePerformanceTelemetry.cs` was not changed because it is required for the final measurement and must preserve M6 comparability.

### Decision 1: Enemy separation query

- **Problem/context:** `EnemyMovement` calls `Physics.OverlapSphereNonAlloc` during movement for enemy separation.
- **AI suggestion:** Consider lower-frequency separation checks or skipping checks in low-population situations.
- **Evidence:** The implementation already uses a reusable 32-entry buffer and therefore avoids per-frame query-array allocation. M6 Hard telemetry reached a peak of 26 active objects, while the configured gameplay maximum is 25; the extra object is explained by death-animation lifecycle counting. No CPU timeline demonstrates that separation is the bottleneck.
- **Decision:** Rejected as an implementation change.
- **Reason:** Changing query frequency, radius, strength, or movement timing could alter enemy movement feel and behavior without sufficient evidence.
- **Validation:** `EnemyMovement.cs` inspected; no code change made.
- **Expected impact:** None claimed. A bounded CPU measurement would be required before revisiting this candidate.

### Decision 2: Rendering and URP settings

- **Problem/context:** Mobile rendering could theoretically spend work on HDR, textures, shadows, additional lights, or post-processing.
- **AI suggestion:** Review and disable only demonstrably unused mobile rendering features.
- **Evidence:** Mobile URP already disables depth and opaque textures, uses MSAA value 1, has an empty renderer feature list, disables additional light shadows, and uses a reduced render scale of 0.8. The scene still contains visual lighting and post-processing-related configuration.
- **Decision:** Rejected as an implementation change.
- **Reason:** There is no GPU timing, overdraw, or thermal evidence supporting a particular setting change. Disabling HDR, main-light shadows, or volume effects could regress visual correctness.
- **Validation:** Mobile URP asset, mobile renderer asset, quality settings, scene settings, and active Unity scene components were inspected. No rendering setting changed.
- **Expected impact:** None claimed. Controlled A/B GPU-oriented measurement is required before changing these settings.

### Decision 3: Asset, texture, mesh, and animation imports

- **Problem/context:** The supplied model tree is approximately 219 MB and includes an approximately 98 MB `enemy.fbx`, separate animation FBX files, textures, and source material data.
- **AI suggestion:** Inspect import settings and consider preserved optimized variants only when unused embedded data is proven.
- **Evidence:** Existing prefabs depend on imported models, Animator controllers, skinned rendering, and existing animation clips. No memory or skinning bottleneck was established by the M6 telemetry.
- **Decision:** Rejected as an implementation change.
- **Reason:** Overwriting or changing supplied imports could break visual appearance, humanoid retargeting, animation compatibility, or material assignment. Original assets must remain preserved.
- **Validation:** Asset sizes, FBX metadata, prefab Animator/renderer usage, and active scene components were inspected. No asset or import setting changed.
- **Expected impact:** None claimed. A controlled import variant and Android memory/rendering comparison would be required.

### Decision 4: Additional per-frame CPU work

- **Problem/context:** `PlayerAutoAttack` can perform a non-allocating overlap scan when it has no valid retained target; pooling still performs component lookup on retrieval, which is low frequency.
- **AI suggestion:** Consider scan cadence changes or additional lookup caching.
- **Decision:** Rejected as an implementation change.
- **Reason:** Player targeting logic and pool semantics are explicitly protected in M7.2. No measured bottleneck justified changing either behavior. M7.1 already addressed the relevant EnemySpawner and EnemyAttack lookup paths.
- **Validation:** `PlayerAutoAttack.cs`, `EnemyPool.cs`, and `EnemySpawner.cs` inspected. No gameplay code changed.
- **Expected impact:** None claimed.

### Decision 5: Runtime telemetry

- **Problem/context:** `RuntimePerformanceTelemetry.PollGameplayState` calls `FindObjectsByType` every 0.25 seconds and rebuilds ID sets.
- **AI suggestion:** Reduce polling or replace it with lifecycle tracking.
- **Decision:** Rejected as an implementation change.
- **Reason:** The telemetry is required for the final Android measurement. Changing sampling semantics would weaken comparability with the M6 baseline and could change reported lifecycle metrics.
- **Validation:** `Assets/Scripts/Performance/RuntimePerformanceTelemetry.cs` inspected and left unchanged.
- **Expected impact:** None claimed during M7.2.

### Applied M7.1 changes carried into M7.2 review

- `EnemySpawner` caches `EnemyAttack` and `EnemyHealth` references.
- `EnemyAttack` no longer performs its per-frame fallback Animator lookup; `Enemy.prefab` references its existing Animator.
- Combat, damage, death, and pool diagnostic logs are guarded for development/editor builds.

These changes were not duplicated or expanded in M7.2. They preserve gameplay ownership and timing.

### Validation summary

- Unity AssetDatabase refresh completed successfully.
- Changed gameplay scripts reported no editor diagnostics.
- Enemy prefab smoke validation passed for Animator wiring and required enemy components.
- Unity scene audit script compiled and ran successfully for cameras, lights, Animators, and SkinnedMeshRenderers.
- No Unity test assembly exists; the test runner returned `No tests found`.
- No Android measurement was performed.
- No large profiler capture was performed.

### Final M7.2 state

- Files changed in M7.2: `Documentation/Optimization.md`, `Documentation/AI_Worklog.md`.
- Gameplay code changed in M7.2: No.
- Rendering/assets/import settings changed in M7.2: No.
- Performance improvement claimed: No.
- Remaining risks: separation query cost at higher populations, GPU cost from lighting/post-processing, and source asset/import footprint.
- Exact next step: **FINAL ANDROID MEASUREMENT AFTER PROJECT COMPLETION**.

## M7.2 - Applied Mobile Optimization Revision

The initial M7.2 pass was intentionally conservative. This revision applied the authorized low-risk optimizations after a direct Enemy prefab audit. No Android benchmark or large profiler capture was performed.

### Enemy separation throttling

- **Context:** Every active enemy performed an allocation-free separation overlap query every frame.
- **AI suggestion:** Cache the last separation vector and refresh it at approximately 10-20 Hz.
- **Change:** `EnemyMovement` refreshes separation every 0.08 seconds while retaining per-frame movement and rotation. `SetTarget` resets the cache for an immediate refresh.
- **Decision:** Accepted.
- **Validation:** Script diagnostics are clean; movement parameters and query buffer remain unchanged.
- **Expected impact:** Lower physics CPU cost with many active enemies; final impact awaits the Android benchmark.

### Enemy renderer optimization

- **Context:** Audit found one Enemy SkinnedMeshRenderer with shadow casting and receiving enabled, blended light/reflection probes, and object motion vectors.
- **AI suggestion:** Disable enemy-only shadow/probe/motion-vector work when it is not needed for readability.
- **Change:** Enemy renderer now has shadow casting off, receive shadows off, light/reflection probes off, and no motion vectors.
- **Decision:** Accepted.
- **Validation:** Unity post-write readback confirmed the values; mesh, materials, bones, and clips were preserved.
- **Expected impact:** Lower enemy GPU rendering work and bandwidth; final impact awaits the Android benchmark.

### Enemy Animator culling

- **Context:** Enemy Animator used `AlwaysAnimate`; movement is script-driven and root motion is disabled.
- **AI suggestion:** Use `CullUpdateTransforms` for off-screen enemies.
- **Change:** Enemy prefab Animator culling mode changed to `CullUpdateTransforms`.
- **Decision:** Accepted.
- **Validation:** Unity readback confirmed culling mode; controller, parameters, update mode, attack timing, and death state were unchanged.
- **Expected impact:** Lower off-screen animation transform cost without changing visible attack/death behavior.

### Enemy material instancing

- **Context:** Repeated enemies share two existing materials, but instancing was disabled.
- **AI suggestion:** Enable instancing on compatible shared materials without creating runtime copies.
- **Change:** Enabled `Material.enableInstancing` on both existing Enemy shared materials.
- **Decision:** Accepted.
- **Validation:** Unity readback confirmed both materials report instancing enabled and the renderer still has two shared materials.
- **Expected impact:** Allows compatible Enemy draws to use GPU instancing; final impact awaits the Android benchmark.

### Deliberately unchanged areas

Global URP shadow/post-processing settings, texture and FBX imports, skin quality, PlayerAutoAttack targeting, physics collision matrix, RuntimePerformanceTelemetry, gameplay limits, and Muzzle Flash were left unchanged. The first group affects broader visual quality, the asset changes lack a safe verified variant, targeting and telemetry are protected contracts, and the remaining gameplay settings would alter behavior rather than optimize implementation.

### Revised final state

- Runtime/rendering optimizations applied: Yes.
- Global project/quality settings changed: No.
- Android benchmark performed: No.
- Large profiler capture performed: No.
- Final performance impact claimed: No.
- Next step: **FINAL ANDROID PERFORMANCE MEASUREMENT**.

## Mobile 60 FPS Push

### Context

The Android device was still slow with many enemies active, and the project had no explicit runtime target frame rate. The user authorized visual-quality reductions to push toward a stable 60 FPS.

### Applied changes

- Added `Assets/Scripts/Performance/MobilePerformanceSettings.cs`.
- Set `Application.targetFrameRate` to 60 and `QualitySettings.vSyncCount` to 0 at `BeforeSceneLoad`.
- Mobile quality: pixel lights 0, low shadow resolution, one shadow cascade, shadow distance 20, anisotropic filtering disabled, realtime GI CPU usage 0, particle raycast budget 64.
- Mobile URP: main shadow resolution 512, shadow distance 20, additional lights disabled and per-object additional-light limit 0.

### Decision and expected impact

Accepted. These settings deliberately trade lighting/shadow/filtering/particle precision for CPU/GPU headroom. Runtime validation confirmed the target and quality values are active. This is not an FPS measurement and no performance improvement is claimed before the final Android benchmark.

### Risk and validation

The main risks are reduced scene shadow quality, flatter lighting, less anisotropic texture quality, and reduced particle collision precision. C# diagnostics are clean, AssetDatabase refresh succeeded, and Play Mode runtime readback reported `targetFps=60`, `vSync=0`, shadow distance 20, Low shadow resolution, one cascade, zero pixel lights, disabled anisotropic filtering, and particle budget 64.

### Intentionally unchanged

`RuntimePerformanceTelemetry.cs`, gameplay limits/timing, PlayerAutoAttack targeting, texture import settings, and final Android measurement remain unchanged. The next step is **FINAL ANDROID PERFORMANCE MEASUREMENT**.
