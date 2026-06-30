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
| `MovementTest.unity` | Flat horizontal test — run, dash, wall jump |
| `VerticalCavernTest.unity` | Vertical shaft inspired by HK cavern maps — climb up through staggered platforms |

Generate scenes via **Hollow** menu if they are missing.

## Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrows | Left Stick |
| Jump | Space | A / South |
| Dash | Left Shift | RT |

## Systems Implemented

- **State machine**: Idle, Run, Jump, Fall, WallSlide, Dash
- **Coyote time** and **jump buffer**
- **Variable jump height** (release Jump early to cut velocity)
- **Wall slide** and **wall jump**
- **Dash** with cooldown (ground + air)
- **Camera follow** with horizontal look-ahead (Cinemachine 3 when available)
- **ScriptableObject config** at `Assets/ScriptableObjects/DefaultHeroConfig.asset`

## Tuning

Edit `DefaultHeroConfig` in the Inspector to adjust run speed, jump force, dash distance, gravity, etc.

## Free Art Assets

Placeholder sprites are generated automatically. For better visuals, import a CC0 pack such as [Kenney Platformer Art](https://kenney.nl/assets/platformer-art-deluxe) into `Assets/Art/` and assign sprites to the Player Visual and platforms.

## Project Structure

```
Assets/Scripts/
  Core/GameEvents.cs
  Input/PlayerInputReader.cs
  Player/HeroController.cs, HeroControllerConfig.cs, HeroStateMachine.cs
  Player/States/   - movement states
  Player/Sensors/  - GroundWallSensor
  Camera/          - CameraLookAhead, SmoothCameraFollow
  Editor/          - HollowSceneSetup (scene generator)
```

## Next Steps (not in MVP)

- Nail combat, HealthManager, enemies
- Silk resource / heal system
- Ability unlocks and metroidvania map
