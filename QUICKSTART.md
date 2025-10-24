# TBS Framework - Quick Start Guide

Get up and running with the TBS Framework in 10 minutes!

## Prerequisites

- Unity 2021.3 or later
- TextMeshPro package installed

## 5-Minute Setup

### Step 1: Verify Scripts (Already Done!)

All 17 core C# scripts are already in `Assets/Scripts/TBS/`:
- ✅ Grid system with A* pathfinding
- ✅ Turn management
- ✅ Combat system with 4 weapon types
- ✅ Unit system with equipment
- ✅ AI controller
- ✅ UI framework
- ✅ Camera controls
- ✅ Save/load system

### Step 2: Create Example Content

1. In Unity Editor, go to **Tools > TBS Framework > Setup Wizard**
2. Click **"Create All Example Content"**
3. This creates:
   - 4 example weapons (Rifle, Sword, Shotgun, Grenade)
   - 2 unit templates (Soldier, Enemy Grunt)

### Step 3: Create the Game Scene

#### A. Create Grid System

1. Create Empty GameObject: "GameSystems"
2. Add component: `GridManager`
3. Create a Tile Prefab:
   - Create Cube: Scale (1, 0.1, 1)
   - Add component: `GridTile`
   - Ensure it has a Collider
   - Drag to Prefabs folder as "GridTile"
4. Assign GridTile prefab to GridManager
5. Set Grid Width: 10, Height: 10

#### B. Add Core Game Systems

On the "GameSystems" object, add these components:
- `TurnManager`
- `CombatSystem`
- `PlayerController`
- `EnemyAI`
- `GameStateManager`
- `GameManager`

#### C. Setup Camera

1. Select Main Camera
2. Add component: `TacticalCamera`
3. Set Position: (5, 15, -5)
4. Set Rotation: (45, 0, 0)

#### D. Create Simple UI

1. Create Canvas (Right-click Hierarchy > UI > Canvas)
2. On Canvas, add component: `GameUIManager`
3. Create UI elements as children of Canvas:
   - **Turn Info:**
     - TextMeshPro Text: "TurnPhaseText" (top-left)
     - TextMeshPro Text: "TurnNumberText" (top-left, below phase)
   - **End Turn Button:**
     - Button: "EndTurnButton" (top-right)
   - **Unit Info Panel:** (right side)
     - Panel: "UnitInfoPanel"
     - Inside panel:
       - TextMeshPro: "UnitNameText"
       - TextMeshPro: "UnitHealthText"
       - TextMeshPro: "UnitActionPointsText"
       - TextMeshPro: "UnitWeaponText"
       - Slider: "HealthBar"
   - **Combat Log:**
     - TextMeshPro Text: "CombatLogText" (bottom-left)
4. Link all these UI elements to GameUIManager component
5. Disable "UnitInfoPanel" initially

#### E. Create Unit Prefab

1. Create Capsule: Scale (0.8, 1, 0.8)
2. Add component: `Unit`
3. Ensure it has a Collider
4. Create Layer "Unit" and assign it
5. Save as Prefab: "UnitPrefab"

#### F. Setup Unit Spawner

1. Create Empty GameObject: "UnitSpawner"
2. Add component: `UnitSpawner`
3. Assign your Unit Prefab
4. Configure Units To Spawn (expand array, set size to 4):

   **Unit 0 (Player):**
   - Unit Name: "Player 1"
   - Faction: Player
   - Spawn Position: (2, 2)
   - Starting Weapon: Rifle (from Resources/Weapons)
   - Unit Color: Blue

   **Unit 1 (Player):**
   - Unit Name: "Player 2"
   - Faction: Player
   - Spawn Position: (3, 2)
   - Starting Weapon: Sword
   - Unit Color: Blue

   **Unit 2 (Enemy):**
   - Unit Name: "Enemy 1"
   - Faction: Enemy
   - Spawn Position: (7, 7)
   - Starting Weapon: Rifle
   - Unit Color: Red

   **Unit 3 (Enemy):**
   - Unit Name: "Enemy 2"
   - Faction: Enemy
   - Spawn Position: (8, 7)
   - Starting Weapon: Shotgun
   - Unit Color: Red

#### G. Setup Layers & Raycasting

1. Create Layers:
   - Layer 6: "Tile"
   - Layer 7: "Unit"

2. Assign layers:
   - All grid tiles: Layer "Tile"
   - All units: Layer "Unit"

3. On PlayerController component:
   - Tile Layer: Select "Tile"
   - Unit Layer: Select "Unit"

### Step 4: Play!

Press Play and you should have:
- A 10x10 grid
- 2 player units (blue) vs 2 enemy units (red)
- Working turn system
- Click to select units
- Click tiles to move
- Click enemies to attack
- End Turn button to switch phases

## Controls

### Keyboard
- **WASD / Arrow Keys:** Pan camera
- **Q/E:** Rotate camera
- **Mouse Scroll:** Zoom in/out
- **ESC:** Deselect unit
- **F5:** Quick save
- **F9:** Quick load

### Mouse
- **Left Click:** Select unit / Move / Attack
- **Right Click:** Deselect unit
- **Screen Edges:** Pan camera (optional)

## Gameplay Flow

1. **Player Turn:**
   - Select a unit (blue)
   - Movement range highlights in blue
   - Click a blue tile to move (costs 1 AP)
   - Click an enemy to attack (costs 1 AP)
   - Each unit has 2 action points
   - Click "End Turn" when done

2. **Enemy Turn:**
   - AI automatically controls red units
   - Enemies move towards and attack player units
   - Turn automatically ends when all enemies acted

3. **Victory/Defeat:**
   - Eliminate all enemies to win
   - Game ends if all player units die

## Next Steps

See `FRAMEWORK_README.md` for:
- Complete architecture documentation
- How to extend the framework
- Adding new weapon types
- Creating custom units
- Advanced AI behaviors
- Custom abilities and status effects

## Troubleshooting

**Units won't spawn:**
- Check that UnitSpawner has the Unit Prefab assigned
- Verify spawn positions are within grid bounds
- Check that weapons are assigned from Resources/Weapons

**Can't select units:**
- Verify PlayerController has correct layer masks
- Check that units have colliders
- Ensure units are on "Unit" layer

**Grid doesn't appear:**
- Check GridManager has tile prefab assigned
- Verify tile prefab has GridTile component
- Check grid size settings

**UI doesn't work:**
- Verify all UI elements are linked in GameUIManager
- Check that TextMeshPro is imported
- Ensure Canvas has GameUIManager component

## File Locations

- **Scripts:** `Assets/Scripts/TBS/`
- **Example Weapons:** `Assets/Resources/Weapons/`
- **Example Units:** `Assets/Resources/Units/`
- **Prefabs:** `Assets/Prefabs/`

## Example Content Created

### Weapons
1. **Rifle** (Ranged)
   - Damage: 30, Range: 6, Accuracy: 85%
   - Accuracy falls off with distance

2. **Sword** (Melee)
   - Damage: 40, Range: 1, Accuracy: 90%
   - High damage, close range only

3. **Shotgun** (Spread)
   - Damage: 25, Range: 3, Accuracy: 75%
   - Affects cone area

4. **Grenade** (AOE)
   - Damage: 35, Range: 5, Accuracy: 100%
   - Affects 2-tile radius

---

**You're ready to build your tactical strategy game! 🎮**
