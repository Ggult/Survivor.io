# Baseline Plan

## Goal

Record a repeatable optimization-before reference before changing runtime or asset performance characteristics.

## Controlled conditions

- Same Unity version and build configuration
- Same device or Editor profile
- Same resolution and quality settings
- Same camera and arena layout
- Same difficulty configuration
- Warm-up excluded from reported values
- At least three repetitions per scenario where practical

## Scenarios

| Scenario | Active enemies | Duration | Notes |
|---|---:|---:|---|
| Low | 20 | 30-60 seconds | Establish idle gameplay cost |
| Medium | 50 | 30-60 seconds | Normal pressure case |
| High | 100 | 30-60 seconds | Stress case |

## Metrics

Primary: FPS, CPU frame time, GPU frame time, GC Alloc, memory, draw calls, SetPass calls, triangles and vertices.

Secondary: script update time, Animator time, Physics time, target detection time and spawn/despawn cost.

## Result table

| Scenario | FPS | CPU ms | GPU ms | GC Alloc | Memory | Draw Calls | SetPass | Triangles | Notes |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---|
| 20 enemies | - | - | - | - | - | - | - | - | Not measured yet |
| 50 enemies | - | - | - | - | - | - | - | - | Not measured yet |
| 100 enemies | - | - | - | - | - | - | - | - | Not measured yet |

## Measurement rule

Measure -> identify one bottleneck -> apply one focused change -> measure again under the same conditions.
