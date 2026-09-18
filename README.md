# Survivor.io Case Study

## Project overview

Mobile Survivor.io-style arena case study developed with Unity, GitHub Copilot and Unity MCP.

The project will use one arena scene, supplied character/enemy/weapon assets, configurable Easy/Medium/Hard difficulty, and a three-minute survival loop.

## Current state

M1 Player Foundation is complete. The scene contains a player prefab instance with movement and health components, plus a camera follow component.

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
| M1 | Player movement and health |
| M2 | Enemy movement, damage and pooling |
| M3 | Auto attack, projectiles and combat |
| M4 | Game flow, timer, result screens and replay |
| M5 | Difficulty and persistent total kill count |
| M6 | Optimization-before baseline |
| M7 | Measurement-driven optimization |
| M8 | End-to-end Unity MCP workflow |
| M9 | Tests and validation |
| M10 | README, APK, video and final delivery |

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

No gameplay baseline has been measured yet. Baseline scenarios and metrics are defined in [BaselinePlan.md](Assets/Documentation/Performance/BaselinePlan.md).

## Known limitations

- Gameplay systems are not implemented yet.
- Android APK has not been produced yet.
- Supplied asset geometry, materials and texture import settings still require measurement.

