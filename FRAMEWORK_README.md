# TBS Framework
## Turn-Based Strategy Game Framework for Unity

A flexible, extensible turn-based strategy game framework inspired by XCOM and BattleTech. Built with Unity C#, this framework provides all the core systems needed to create tactical strategy games.

---

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Quick Start](#quick-start)
- [Core Systems](#core-systems)
- [Extending the Framework](#extending-the-framework)
- [Example Usage](#example-usage)
- [Best Practices](#best-practices)

---

## Features

### Core Turn System
- Two-action-per-turn system with action point tracking
- Player phase → Enemy phase cycling
- Action point validation before execution
- Turn event system for state tracking

### Grid-Based Movement
- Tile-based grid system (adaptable to different tile types)
- A* pathfinding algorithm
- Movement range visualization
- Reachability calculations respecting obstacles

### Cover System
- Three cover levels: None, Half, Full
- Line-of-sight calculations
- Cover affects hit chance in combat
- Visual indicators for cover positions

### Combat System
- **Four weapon types:**
  - **Ranged:** Single-target with distance-based accuracy falloff
  - **Melee:** Close-range, high accuracy attacks
  - **Spread:** Shotgun-style cone attacks affecting multiple targets
  - **AOE:** Area-of-effect attacks (grenades, explosives)
- Hit chance calculation (distance, cover, unit stats)
- Damage variance for combat variety
- Attack range visualization

### Unit System
- Base stats: Health, Movement, Accuracy, Defense
- Equipment system for weapons
- Stat modification through equipment
- Player and AI-controlled unit support
- Faction system

### AI System
- Simple but effective enemy AI
- Move towards nearest target behavior
- Attack when in range
- Extensible for advanced behaviors

### UI Framework
- Turn phase indicator
- Unit selection highlighting
- Action point display
- Combat log
- Health bars

### Camera System
- Tactical camera with pan, zoom, rotation
- WASD + arrow key movement
- Mouse scroll zoom
- Q/E rotation
- Screen edge panning (optional)

### Save/Load Architecture
- Game state serialization
- JSON-based save files
- Extensible state management
- Quick save/load (F5/F9)

---

## Architecture Overview

### Namespace Structure

```
TBS/
├── Core/           - Game managers, turn system, player controller
├── Grid/           - Grid management, tiles, pathfinding
├── Units/          - Unit classes and behaviors
├── Combat/         - Weapon system, combat calculations
├── AI/             - Enemy AI controllers
├── UI/             - User interface components
├── Camera/         - Camera controls
├── Events/         - Event system for decoupled communication
├── Data/           - ScriptableObject templates
└── Utils/          - Utility classes and helpers
```

### Key Design Patterns

1. **Event-Driven Architecture:** The `GameEvents` system allows decoupled communication between systems
2. **Component-Based Design:** Systems are modular and can be mixed and matched
3. **ScriptableObject Data:** Weapons and units use ScriptableObjects for easy content creation
4. **Singleton Pattern:** GameManager provides centralized access to core systems

---

## Quick Start

### Setting Up a New Scene

1. **Create a new Unity scene**

2. **Set up the Grid:**
   - Create an empty GameObject named "GameSystems"
   - Add the `GridManager` component
   - Create a tile prefab:
     - Create a Cube (Scale: 1, 0.1, 1)
     - Add `GridTile` component
     - Add a Collider for raycasting
     - Save as prefab
   - Assign the tile prefab to GridManager
   - Configure grid size (e.g., 10x10)

3. **Add Core Systems to "GameSystems":**
   - `TurnManager`
   - `CombatSystem`
   - `PlayerController`
   - `EnemyAI`
   - `GameStateManager`
   - `GameManager`

4. **Set up the Camera:**
   - Select Main Camera
   - Add `TacticalCamera` component
   - Position camera above the grid (e.g., position: 0, 15, -10, rotation: 45, 0, 0)

5. **Create UI:**
   - Create Canvas (Screen Space - Overlay)
   - Add `GameUIManager` component
   - Add UI elements:
     - Turn Phase Text (TextMeshPro)
     - Turn Number Text
     - End Turn Button
     - Unit Info Panel (initially inactive)
     - Combat Log Text
   - Link UI elements to GameUIManager

6. **Create Unit Prefab:**
   - Create a Capsule (Scale: 0.8, 1, 0.8)
   - Add `Unit` component
   - Add a Collider with "Unit" layer
   - Save as prefab
   - Add to UnitSpawner

7. **Create Example Content:**
   - Use `Tools > TBS Framework > Setup Wizard`
   - Click "Create All Example Content"
   - This creates example weapons and units

8. **Configure UnitSpawner:**
   - Create empty GameObject named "UnitSpawner"
   - Add `UnitSpawner` component
   - Assign unit prefab
   - Configure units to spawn (2 player, 2 enemy)

9. **Configure Layers:**
   - Create layer "Tile" for grid tiles
   - Create layer "Unit" for units
   - Assign layers appropriately

10. **Press Play!**

---

## Core Systems

### Grid System

**Files:** `GridTile.cs`, `GridManager.cs`, `Pathfinding.cs`

The grid system manages the tactical battlefield.

```csharp
// Get a tile at a position
GridTile tile = gridManager.GetTile(new Vector2Int(5, 5));

// Find path between two points
List<GridTile> path = gridManager.FindPath(startPos, endPos);

// Get reachable tiles within movement range
List<GridTile> reachable = gridManager.GetReachableTiles(unitPos, movementRange);

// Check line of sight
bool canSee = gridManager.HasLineOfSight(fromPos, toPos);

// Highlight tiles
gridManager.HighlightMovementRange(position, range);
gridManager.HighlightAttackRange(position, range);
gridManager.ClearAllHighlights();
```

**Extending the Grid:**
- Modify `GridTile.cs` to add tile properties (terrain type, movement cost, etc.)
- Extend `Pathfinding.cs` to account for movement costs
- Add different tile types (water, obstacles, high ground)

### Turn Management

**File:** `TurnManager.cs`

Manages turn phases and action points.

```csharp
// Register units
turnManager.RegisterPlayerUnit(unit);
turnManager.RegisterEnemyUnit(unit);

// End current phase
turnManager.EndCurrentPhase();

// Check current phase
bool isPlayerTurn = turnManager.IsPlayerTurn();
```

**Extending Turns:**
- Add more phases (neutral turn, environmental effects)
- Implement initiative system
- Add time limits for turns

### Combat System

**Files:** `WeaponData.cs`, `CombatSystem.cs`

Handles all combat calculations.

```csharp
// Execute an attack
AttackResult result = combatSystem.ExecuteAttack(attacker, target);

// Check if attack is valid
bool canAttack = combatSystem.CanAttack(attacker, target, out string reason);
```

**Creating New Weapons:**

1. In Unity: `Assets > Create > TBS > Weapon Data`
2. Configure weapon properties
3. Assign to unit via code or inspector

**Weapon Types:**

- **Ranged:** Distance-based accuracy falloff
  ```csharp
  hitChance -= distance * accuracyFalloffPerTile;
  ```

- **Melee:** Adjacent tiles only, high base accuracy
  ```csharp
  // Range = 1, no distance penalty
  ```

- **Spread:** Affects cone of tiles
  ```csharp
  // Set spreadAngle to define cone width
  ```

- **AOE:** Affects circular area
  ```csharp
  // Set aoeRadius to define blast radius
  ```

**Extending Combat:**
- Add critical hits
- Add status effects (stun, poison, etc.)
- Add armor penetration
- Add reaction fire/overwatch

### Unit System

**File:** `Unit.cs`

Base class for all units.

```csharp
// Move a unit
unit.MoveTo(targetPosition, actionPointCost);

// Attack requires combat system
combatSystem.ExecuteAttack(unit, target);

// Equip weapon
unit.EquipWeapon(weaponData);

// Take damage
unit.TakeDamage(amount);

// Heal
unit.Heal(amount);
```

**Creating Custom Unit Types:**

```csharp
using TBS.Units;

public class HeavyUnit : Unit
{
    // Override or extend base functionality
    public override void TakeDamage(int damage)
    {
        // Heavy units take 50% less damage
        base.TakeDamage(damage / 2);
    }
}
```

### Event System

**File:** `GameEvents.cs`

Decoupled event system for game state changes.

```csharp
// Subscribe to events
GameEvents.OnUnitSelected += HandleUnitSelected;
GameEvents.OnAttackExecuted += HandleAttackExecuted;
GameEvents.OnTurnPhaseChanged += HandlePhaseChanged;

// Unsubscribe (important!)
void OnDestroy()
{
    GameEvents.OnUnitSelected -= HandleUnitSelected;
    // ... unsubscribe all
}

// Trigger events (usually done by systems)
GameEvents.TriggerUnitSelected(unit);
GameEvents.TriggerAttackExecuted(attacker, target, result);
```

**Available Events:**
- Turn events: Phase changes, turn number changes
- Unit events: Selection, movement, death, action points
- Combat events: Attacks, damage, healing
- UI events: Game messages

**Adding New Events:**

```csharp
// In GameEvents.cs
public static event Action<YourDataType> OnYourEvent;

public static void TriggerYourEvent(YourDataType data)
{
    OnYourEvent?.Invoke(data);
}
```

### AI System

**File:** `EnemyAI.cs`

Simple but effective AI for enemy units.

**Current Behavior:**
1. Find nearest player unit
2. If in attack range → attack
3. Else → move towards target

**Extending AI:**

```csharp
// Override ProcessUnitTurn for custom behavior
private bool ProcessUnitTurn(Unit enemy, List<Unit> playerUnits)
{
    // Custom AI logic here
    // - Defensive behavior (seek cover)
    // - Flanking maneuvers
    // - Target prioritization
    // - Ability usage

    return actionTaken;
}
```

**Advanced AI Ideas:**
- Cover seeking behavior
- Flanking tactics
- Target priority (low health, high threat)
- Ability cooldowns
- Cooperative tactics

---

## Extending the Framework

### Adding New Weapon Types

1. Add weapon type to enum in `WeaponData.cs`:
```csharp
public enum WeaponType
{
    Ranged,
    Melee,
    Spread,
    AOE,
    YourNewType  // Add here
}
```

2. Add execution logic in `CombatSystem.cs`:
```csharp
switch (weapon.Type)
{
    // ... existing cases
    case WeaponType.YourNewType:
        result = ExecuteYourNewTypeAttack(attacker, target);
        break;
}
```

3. Implement the attack method:
```csharp
private AttackResult ExecuteYourNewTypeAttack(Unit attacker, Unit target)
{
    // Your custom attack logic
    AttackResult result = new AttackResult();
    // ... calculate and apply damage
    return result;
}
```

### Adding Unit Abilities

1. Create ability data class:
```csharp
[CreateAssetMenu(fileName = "New Ability", menuName = "TBS/Ability")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public int actionPointCost;
    public int cooldownTurns;
    // ... other properties
}
```

2. Extend Unit class:
```csharp
public class Unit : MonoBehaviour
{
    [SerializeField] private List<AbilityData> abilities;
    private Dictionary<AbilityData, int> abilityCooldowns;

    public void UseAbility(AbilityData ability, Vector2Int target)
    {
        // Implement ability logic
    }
}
```

3. Create ability system:
```csharp
public class AbilitySystem : MonoBehaviour
{
    public void ExecuteAbility(Unit caster, AbilityData ability, Vector2Int target)
    {
        // Ability execution logic
    }
}
```

### Adding Status Effects

1. Create status effect class:
```csharp
public enum StatusEffectType
{
    Stun, Poison, Burning, Shielded, Hasted
}

[Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int duration;
    public int value;

    public void ApplyEffect(Unit unit)
    {
        // Effect logic per turn
    }
}
```

2. Extend Unit class:
```csharp
private List<StatusEffect> activeEffects = new List<StatusEffect>();

public void AddStatusEffect(StatusEffect effect)
{
    activeEffects.Add(effect);
}

public void UpdateStatusEffects()
{
    foreach (var effect in activeEffects)
    {
        effect.ApplyEffect(this);
        effect.duration--;
    }
    activeEffects.RemoveAll(e => e.duration <= 0);
}
```

### Creating Custom Grid Tiles

```csharp
public class SpecialTile : GridTile
{
    public enum TileType
    {
        Normal, Water, HighGround, Hazard
    }

    [SerializeField] private TileType tileType;

    public override int GetMovementCost()
    {
        switch (tileType)
        {
            case TileType.Water: return 2;
            case TileType.HighGround: return 2;
            case TileType.Hazard: return 3;
            default: return 1;
        }
    }
}
```

---

## Example Usage

### Creating a Custom Game Mode

```csharp
using TBS.Core;
using TBS.Units;

public class CustomGameMode : MonoBehaviour
{
    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        SetupCustomRules();
    }

    void SetupCustomRules()
    {
        // Example: 3 action points per turn instead of 2
        // You would modify TurnManager or override its behavior

        // Example: Victory condition
        GameEvents.OnUnitDied += CheckVictoryCondition;
    }

    void CheckVictoryCondition(Unit deadUnit)
    {
        // Custom victory logic
    }
}
```

### Spawning Units Dynamically

```csharp
using TBS.Utils;
using TBS.Combat;

public class WaveSpawner : MonoBehaviour
{
    public GameObject unitPrefab;
    public WeaponData enemyWeapon;

    void SpawnWave(int waveNumber)
    {
        int enemyCount = 2 + waveNumber;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int spawnPos = FindSpawnPosition();
            UnitSpawner.QuickSpawn(
                unitPrefab,
                spawnPos,
                UnitFaction.Enemy,
                enemyWeapon,
                gridManager
            );
        }
    }
}
```

### Custom UI Integration

```csharp
using TBS.Events;
using TBS.Units;

public class CustomUIPanel : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.OnAttackExecuted += DisplayAttackFeedback;
    }

    void OnDisable()
    {
        GameEvents.OnAttackExecuted -= DisplayAttackFeedback;
    }

    void DisplayAttackFeedback(Unit attacker, Unit target, AttackResult result)
    {
        // Show custom attack animation or feedback
        if (result.Hit)
        {
            ShowDamageNumber(target.transform.position, result.Damage);
        }
        else
        {
            ShowMissIndicator(target.transform.position);
        }
    }
}
```

---

## Best Practices

### Performance

1. **Object Pooling:** For frequently spawned objects (damage numbers, effects)
2. **Pathfinding Caching:** Cache paths when possible
3. **Event Cleanup:** Always unsubscribe from events in OnDestroy
4. **Grid Optimization:** Use spatial hashing for large grids

### Code Organization

1. **Use Namespaces:** Keep code organized with the TBS namespace structure
2. **ScriptableObjects:** Use for data that designers should configure
3. **Separation of Concerns:** Keep rendering separate from game logic
4. **Event-Driven:** Use events instead of direct references when possible

### Extensibility

1. **Virtual Methods:** Make methods virtual if they might be overridden
2. **Interfaces:** Use interfaces for systems that might have multiple implementations
3. **Composition:** Favor composition over inheritance
4. **Documentation:** Comment non-obvious code and public APIs

### Testing

1. **Unit Tests:** Test pathfinding, combat calculations separately
2. **Debug Visualization:** Use Gizmos to visualize ranges, paths
3. **Editor Tools:** Create editor scripts for quick testing
4. **Logging:** Use GameEvents.TriggerGameMessage for gameplay logging

---

## File Structure

```
Assets/
├── Scripts/
│   └── TBS/
│       ├── Core/
│       │   ├── GameManager.cs
│       │   ├── TurnManager.cs
│       │   ├── PlayerController.cs
│       │   └── GameStateManager.cs
│       ├── Grid/
│       │   ├── GridTile.cs
│       │   ├── GridManager.cs
│       │   └── Pathfinding.cs
│       ├── Units/
│       │   └── Unit.cs
│       ├── Combat/
│       │   ├── WeaponData.cs
│       │   └── CombatSystem.cs
│       ├── AI/
│       │   └── EnemyAI.cs
│       ├── UI/
│       │   └── GameUIManager.cs
│       ├── Camera/
│       │   └── TacticalCamera.cs
│       ├── Events/
│       │   └── GameEvents.cs
│       ├── Data/
│       │   └── UnitData.cs
│       └── Utils/
│           ├── UnitSpawner.cs
│           └── Editor/
│               └── TBSSetupWizard.cs
├── Resources/
│   ├── Weapons/
│   │   ├── Rifle.asset
│   │   ├── Sword.asset
│   │   ├── Shotgun.asset
│   │   └── Grenade.asset
│   └── Units/
│       ├── Soldier.asset
│       └── Enemy Grunt.asset
├── Prefabs/
│   ├── Grid/
│   │   └── GridTile.prefab
│   ├── Units/
│   │   ├── PlayerUnit.prefab
│   │   └── EnemyUnit.prefab
│   └── UI/
│       └── GameUI.prefab
└── Scenes/
    └── TacticalBattle.unity
```

---

## System Requirements

- Unity 2021.3 or later
- TextMeshPro package
- Input System package (included)

---

## Credits

**TBS Framework** - A flexible turn-based strategy framework for Unity
Created as a foundation for tactical strategy games inspired by XCOM and BattleTech.

Built with:
- Unity 6000.2.9f1
- Universal Render Pipeline
- C# .NET Standard 2.1

---

## License

This framework is provided as-is for educational and commercial use.
Feel free to extend, modify, and build upon this foundation.

---

## Support & Contributing

For questions or contributions:
1. Review the code comments and documentation
2. Check the example implementations
3. Extend the framework to suit your needs

**Happy developing!** 🎮
