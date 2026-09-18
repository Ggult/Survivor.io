# M0 - Project Setup

## Verified state

- Unity version: 6000.0.70f1
- URP package: 17.0.4
- Input System: 1.19.0
- Test Framework: 1.6.0
- Unity MCP: 0.91.0
- Active scene: `Assets/Scenes/SampleScene.unity`
- Initial scene roots: Main Camera, Directional Light, Global Volume
- No gameplay C# scripts found under `Assets` at M0 start

## M0 decisions

- Keep one arena scene.
- Keep supplied source assets unchanged.
- Separate original and optimized asset destinations.
- Record measurements before making optimization changes.
- Use the simplest architecture that satisfies the case study.

## M0 validation

- Unity MCP listed the registered tools.
- Unity MCP read the active scene and confirmed it was valid and saved.
- Project packages and asset inventory were inspected before creating this structure.
