# Survivor.io Case Study

## Project overview

Mobile Survivor.io-style arena case study developed with Unity, GitHub Copilot and Unity MCP.

The project will use one arena scene, supplied character/enemy/weapon assets, configurable Easy/Medium/Hard difficulty, and a three-minute survival loop.

## Current state

The playable case-study loop is complete in one arena scene. The player moves with a virtual joystick, attacks enemies automatically, enemies spawn in configurable waves, deal damage, and are pooled. The run ends after three minutes or when the player dies, and the result panel supports replay and kill totals.

- Unity: `6000.0.70f1`
- Render pipeline: URP `17.0.4`
- Input System: `1.19.0`
- Test Framework: `1.6.0`
- Unity MCP: `0.91.0`
- Active scene: `Assets/Scenes/SampleScene.unity`

The initial scene contains the default Main Camera, Directional Light and Global Volume. The supplied source assets remain under `Assets/case_models` and will not be modified directly.

## Development principles

- Keep one arena scene and data-driven difficulty settings.
- Prefer readable, small systems over unnecessary frameworks.
- Preserve original assets and place optimized copies separately.
- Measure before optimization and validate the same scenario afterward.
- Record important AI decisions and MCP workflows in `Assets/Documentation`.

## Milestones

| Milestone | Scope |
|---|---|
| M0 | Project setup, inventory and documentation |
| M1-M5 | Player, enemies, combat, game flow, replay, difficulty and persistent total kills |
| M6 | Android runtime baseline before optimization |
| M7.1-M7.2 | Measurement-driven mobile CPU/GPU/model optimizations |
| M8 | End-to-end Unity MCP workflow and validation |
| M9 | Manual runtime validation; no automated test assembly is currently included |
| M10 | Repository delivery documentation; APK/video remain external delivery artifacts |

## Documentation

- [M0 project setup](Assets/Documentation/M0-ProjectSetup.md)
- [M1 player foundation](Assets/Documentation/M1-PlayerFoundation.md)
- [Asset inventory](Assets/Documentation/AssetInventory.md)
- [Baseline measurement plan](Assets/Documentation/Performance/BaselinePlan.md)
- [AI worklog template](Assets/Documentation/AIWorklog/DecisionTemplate.md)

## Planned controls

Virtual joystick movement is implemented in `MobileHUD/VirtualJoystick`; its event is consumed by `MovementInputConsumer`, which exposes the current vector to `PlayerMovement`.

## Packages and tools

The project uses URP, Input System, Unity Test Framework and Unity MCP. Exact package versions are recorded in `Packages/manifest.json`.

## Performance status

Android runtime measurements were captured on a Huawei COR-L29 using the same telemetry format before and after optimization. The optimized Medium run reached 60.02 FPS average and the optimized Hard run reached 59.02 FPS average. The exact reports are stored under [BeforeOptimization](Assets/PerformanceAnalysis/BeforeOptimization) and [AfterOptimization](Assets/PerformanceAnalysis/AfterOptimization). The comparison and optimization rationale are documented in [Optimization.md](Documentation/Optimization.md).

## AI and MCP workflow

The project used GitHub Copilot and Unity MCP for scene/prefab inspection, asset and renderer changes, compilation refreshes, runtime smoke checks, and validation readbacks. The decisions, accepted changes, rejected alternatives, and evidence are recorded in [AI_Worklog.md](Documentation/AI_Worklog.md).

## Validation status

- Build scene: `Assets/Scenes/SampleScene.unity` is enabled in `EditorBuildSettings.asset`.
- Difficulty configurations: Easy, Medium and Hard use the same arena and expose enemy count/spawn interval through `DifficultyConfig` assets.
- Persistence: lifetime enemy kills are stored with `PlayerPrefs` under `Survivor.TotalKills` and shown on the result panel.
- Pooling: enemies are prewarmed and returned to `EnemyPool` instead of repeatedly instantiated and destroyed during play.
- Android: before/after runtime reports are present; no Unity Profiler capture was used.
- Automated tests: no Unity test assembly is currently present; manual Unity/MCP smoke validation is documented.

## Known limitations and external delivery items

- An APK is not committed to the repository; build it from the enabled scene using the Unity Android build target before sending the submission email.
- The requested 3-5 minute video and the final repository/APK link email are external submission steps and are not generated inside the Unity project.
- Runtime telemetry does not provide a CPU/GPU timeline or graphics-driver memory value; those fields remain unavailable on the target device.
- The optimized models intentionally trade silhouette/deformation fidelity for mobile performance. Original supplied FBX files remain preserved under `Assets/case_models`.

