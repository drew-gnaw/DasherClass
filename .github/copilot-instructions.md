# DasherClass Mod - AI Coding Guidelines

## Overview
This is a tModLoader mod for Terraria that introduces a "Dasher Class" - a new player class focused on dash-based combat. Weapons charge up and perform lunging attacks with unique projectile behaviors.

## Architecture
- **Core Components**: `DasherClass.cs` (main mod), `DasherPlayer.cs` (player mechanics), `DasherDamageClass.cs` (custom damage type)
- **Weapon System**: Items in `Items/Weapons/` spawn projectiles from `Projectiles/` that handle charging visuals and dash execution
- **Projectile Hierarchy**: Abstract base `DashWeaponProjectile` with concrete implementations like `ShieldWeaponProjectile` for collision behaviors
- **Data Flow**: Weapons set `Item.shoot` to projectile type; `Shoot()` spawns projectile; projectile AI manages charge → lunge → collision

## Key Patterns
- **Dash Mechanics**: Weapons use `Item.channel = true`, `noMelee = true`, `noUseGraphic = true`. Projectiles charge via `currentChargeTime`, release triggers `PerformLunge()` setting `Owner.velocity` and `DasherPlayer.isLunging`
- **Collision Handling**: Shield weapons (`ShieldWeaponProjectile`) call `ReelBack()` on hit, reflecting player velocity
- **Damage Class**: All weapons use `DasherDamageClass.Instance` for consistent damage scaling
- **Localization**: Implement `ILocalizedModType` with `LocalizationCategory` matching hjson file structure (e.g., "Items.Weapons")

## Examples
- Basic shield weapon: `BasicDefensiveMagic.cs` + `BasicDefensiveMagicDash.cs` (inherits `ShieldWeaponProjectile`)
- Lance with multi-projectile: `EtherealLance.cs` spawns additional projectiles during charge
- Player state: `DasherPlayer.PreUpdate()` modifies `maxFallSpeed` during lunges

## Development Workflow
- **Build**: Use tModLoader's build system (run mod in-game or via tModLoader CLI)
- **Test**: Spawn items via creative mode, test charge times and dash distances in-game
- **Balance**: Adjust values in `Balancing/` folder, update recipes in weapon `AddRecipes()`
- **Localization**: Edit `.hjson` files in `Localization/en-US/`, reload mod to see changes

## Conventions
- Namespace: `DasherClass` root, subnamespaces like `DasherClass.DasherPlayer`
- Projectile AI: Use `Projectile.ai[0]` for state flags, `ai[1]` for timers
- IFrames: Grant via `Owner.GiveUniversalIFrames()` on dash start
- Particles: Generate dust in `GenerateChargingParticles()` and `GenerateChargedParticle()` for visual feedback</content>
<parameter name="filePath">c:\Users\andre\OneDrive\Documents\My Games\Terraria\tModLoader\ModSources\DasherClass\.github\copilot-instructions.md