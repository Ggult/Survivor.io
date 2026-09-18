# M1 Player Foundation

## Scope

- Player model instantiated from `Assets/case_models/player.fbx`.
- Reusable prefab created at `Assets/Prefabs/Player.prefab`.
- Movement, health and camera follow components added.
- Top-down camera offset configured to `(0, 14, -10)`.
- Static `25x25` ground plane added at `Y=-0.1`.
- Screen-space mobile joystick and Input System UI EventSystem added.
- Input dependency separated through `MovementInputConsumer`; the joystick does not reference `PlayerMovement` or gameplay state.
- Original supplied asset remains unchanged.

## Runtime behavior

`PlayerMovement` moves on the XZ arena plane, clamps position to the configured arena half extent and consumes `MovementInputConsumer.MoveInput`. `VirtualJoystick` only publishes its `MovementInputChanged` event; the consumer owns the subscription lifecycle and stores the latest vector.

`PlayerHealth` initializes to the configured maximum health, accepts positive damage and clamps health at zero. Death state is exposed through `IsDead`; game-over behavior belongs to a later milestone.

`PlayerCameraFollow` follows a target with a configurable offset and smoothing. The active scene camera targets the player instance.

## Asset evidence

The supplied `player` texture is `512x512`, imported as `DXT1` with 10 mip levels. No texture optimization was applied in M1 because there is not yet a measured rendering or memory bottleneck.

## Validation

- AssetDatabase refresh completed successfully.
- Unity Error log was empty after script compilation and scene setup.
- `SampleScene` saved successfully and is not dirty.
- EditMode test discovery completed; no test assembly exists yet.

## Deferred work

- Player animation and facing rules.
- Damage feedback and game-over integration.
- Camera tuning after the arena layout is established.