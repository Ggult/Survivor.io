# Skyloft Studios Case Study Delivery Checklist

## Repository requirements

- [x] One playable arena scene: `Assets/Scenes/SampleScene.unity`
- [x] Virtual joystick movement
- [x] Automatic ranged attack
- [x] Enemy waves, approach, damage and pooling
- [x] Three-minute survival objective
- [x] Game Over and Victory result panel
- [x] Run kill count and replay action
- [x] Lifetime kill count persisted with `PlayerPrefs`
- [x] Easy, Medium and Hard data-driven difficulty configurations
- [x] Supplied Player, Enemy and Rifle assets used
- [x] Original source assets preserved
- [x] Optimized model variants stored under `Assets/case_models/Optimized`
- [x] Before/after Android runtime measurements
- [x] AI/MCP decisions and validation evidence
- [x] Package versions and active build scene documented

## Validation evidence

- Unity diagnostics are clean for the touched gameplay/UI scripts.
- Play Mode startup smoke confirmed `WaitingForDifficulty`, visible difficulty selection and hidden result panel.
- Android reports are stored in:
  - `Assets/PerformanceAnalysis/BeforeOptimization`
  - `Assets/PerformanceAnalysis/AfterOptimization`
- The optimized Medium report reached 60.02 FPS average.
- The optimized Hard report reached 59.02 FPS average.
- No Unity automated test assembly is included; manual runtime and MCP validation are documented.

## External delivery steps

- [x] Android APK generated at `Builds/SurvivorCaseStudy.apk` (approximately 55.3 MB); device install/play smoke test remains recommended.
- [ ] Record the requested 3-5 minute gameplay and Unity MCP workflow video.
- [ ] Push the final repository state and identify the reference/optimized boundary with commits or tags.
- [ ] Email repository link, APK, README, AI worklog and video to `main@skyloftstudios.com`.
