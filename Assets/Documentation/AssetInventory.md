# Asset Inventory

## Project context

- Unity: 6000.0.70f1
- Render pipeline: URP 17.0.4
- Source asset folder: `Assets/case_models`
- Original files are preserved in place until import and runtime measurements justify a copy.

## Initial inventory

| Asset | Type | Initial observation | Next measurement |
|---|---|---|---|
| `enemy.fbx` | Model | Approximately 100 MB on disk; highest-priority inspection target | Mesh, material, rig, animation and imported texture data |
| `player.fbx` | Model | Approximately 5.4 MB on disk | Mesh, material, rig and animation data |
| `rifle.fbx` | Model | Approximately 193 KB on disk | Mesh and material data |
| `enemy.jpg` | Texture | Source preview texture | Resolution, format and runtime usage |
| `player.jpg` | Texture | Source preview texture | Resolution, format and runtime usage |
| `enemy.fbm/*` | Texture set | Diffuse, normal, specular and glossiness maps | Import settings, dimensions and memory |
| `player.fbm/*` | Texture set | Body/head diffuse, normal and specular maps | Import settings, dimensions and memory |
| `rifle_text/*` | Texture set | Albedo, metallic/smoothness and normal maps | Import settings, dimensions and memory |

## Rules

- Do not modify files under `Assets/case_models` directly.
- Copy an asset to `Assets/Art/Optimized` only after recording its original import and performance data.
- Keep visual validation and performance measurements for every optimization decision.
