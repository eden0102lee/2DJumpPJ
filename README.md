# Hollow - Silksong Style Movement MVP

Unity 6 2D platformer movement prototype inspired by Hollow Knight: Silksong.

## Quick Start

1. Open `2DJumpPJ` in Unity 6000.0.39f1 (or newer Unity 6).
2. Wait for packages to resolve (Cinemachine will install automatically).
3. Use menu **Hollow > Setup Vertical Cavern Test Scene** (or **Setup Movement Test Scene** for the flat test).
4. Open the scene and press Play.

### Scenes

| Scene | Description |
|-------|-------------|
| `MovementTest.unity` | Flat horizontal test — run, sprint, dash, wall jump, down-dash |
| `VerticalCavernTest.unity` | Vertical shaft inspired by HK cavern maps — climb, wall jump combos |

Generate scenes via **Hollow** menu if they are missing.

## Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrows | Left Stick |
| Jump | Space | A / South |
| Dash (tap) | C / Left Shift | RT (tap) |
| Sprint (hold) | Hold C / Shift | Hold RT |

### Movement Techniques

| Technique | Input |
|-----------|-------|
| **Dash** | Tap Sprint while grounded or airborne |
| **Sprint** | Hold Sprint while moving on ground |
| **Sprint Jump** | Jump while sprinting (higher and farther) |
| **Wall Cling** | Hold toward wall while airborne |
| **Wall Jump** | Jump while clinging to wall |
| **Wall Climb** | Hold Sprint + toward wall while clinging |
| **Wall Up-Dash** | Tap Sprint while clinging to wall |
| **Down Dash** | Hold Down + tap Sprint while airborne |
| **Wall Jump + Air Dash** | Wall jump, then immediately tap Sprint away from wall |

## Systems Implemented

- **State machine**: Idle, Run, Sprint, Jump, Fall, WallSlide, WallClimb, Dash, DownDash
- **Swift Step**: tap-to-dash / hold-to-sprint with sprint jump bonus
- **Cling Grip**: wall cling delay, slide, wall jump, wall climb, wall up-dash
- **Coyote time** and **jump buffer**
- **Variable jump height** (release Jump early to cut velocity)
- **Dash i-frames** and end-of-dash momentum carry
- **Down dash** (hold to continue until release)
- **Bilateral wall detection** (cling by pressing toward wall)
- **Camera follow** with sprint look-ahead (Cinemachine 3 when available)
- **ScriptableObject config** at `Assets/ScriptableObjects/DefaultHeroConfig.asset`

## Tuning

Edit `DefaultHeroConfig` in the Inspector. Key feel targets (1 unit ≈ 1 tile):

| Metric | Target | Config fields |
|--------|--------|---------------|
| Single jump height | ~2.8–3.2 units | `jumpForce`, `gravityScale`, `lowJumpGravityMultiplier` |
| Sprint jump height | ~3.5–4 units | `sprintJumpForce`, `sprintJumpHorizontalBoost` |
| Dash distance | ~3.2–3.8 units | `dashSpeed` × `dashDuration` |
| Wall jump height | ~2.5 units | `wallJumpForce` |
| Sprint vs run speed | ~1.6× | `sprintSpeed` / `runSpeed` |
| Coyote / buffer | 120 ms | `coyoteTime`, `jumpBufferTime` |

## Free Art Assets

Placeholder sprites are generated automatically. For better visuals, import a CC0 pack such as [Kenney Platformer Art](https://kenney.nl/assets/platformer-art-deluxe) into `Assets/Art/` and assign sprites to the Player Visual and platforms.

## Project Structure

```
Assets/Scripts/
  Core/GameEvents.cs
  Input/PlayerInputReader.cs
  Player/HeroController.cs, HeroControllerConfig.cs, HeroStateMachine.cs
  Player/States/   - movement states (Idle, Run, Sprint, Jump, Fall, WallSlide, WallClimb, Dash, DownDash)
  Player/Sensors/  - GroundWallSensor
  Camera/          - CameraLookAhead, SmoothCameraFollow
  Editor/          - HollowSceneSetup (scene generator)
```

## Next Steps (not in MVP)

- Nail combat, HealthManager, enemies
- Silk resource / heal system
- Ability unlocks and metroidvania map
- Double jump (Faydown Cloak), glide (Drifter's Cloak)
