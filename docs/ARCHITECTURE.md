# Architecture Guide - Submarine Monster Tactical Game

**Version**: 1.4
**Last Updated**: 2025-11-22

---

## Overview

This document describes the architectural design of the Submarine Monster Tactical Game. The architecture prioritizes:

- **Separation of Concerns**: Core game logic is completely independent of Unity
- **Testability**: All game logic can be unit tested without Unity runtime
- **Multiplayer-Ready**: Designed for future networked multiplayer from day one
- **Maintainability**: Clear boundaries between systems, well-documented interfaces

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    UNITY PRESENTATION LAYER                  │
│                    (SubGame.Unity assembly)                  │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │GridVisualizer│  │ SubmarineView│  │ MonsterView  │      │
│  │              │  │   (future)   │  │   (future)   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│                                                              │
│  - Renders game state visually                              │
│  - Handles Unity-specific input                             │
│  - Subscribes to Core events                                │
│  - NO game logic here                                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ References (one-way)
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    CORE GAME LOGIC                          │
│              (SubGame.Core assembly - NO UNITY)             │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                    Grid System                       │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌────────────┐  │   │
│  │  │GridCoordinate│  │ IGridSystem │  │ GridSystem │  │   │
│  │  │   (struct)   │  │ (interface) │  │  (class)   │  │   │
│  │  └─────────────┘  └─────────────┘  └────────────┘  │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                  Entity System                       │   │
│  │  ┌─────────┐  ┌─────────┐  ┌───────────────────┐   │   │
│  │  │ IEntity │  │ Entity  │  │ EntityManager     │   │   │
│  │  │(interf.)│  │ (base)  │  │ (tracks entities) │   │   │
│  │  └─────────┘  └─────────┘  └───────────────────┘   │   │
│  │  ┌─────────┐  ┌─────────┐  ┌───────────────────┐   │   │
│  │  │Submarine│  │ Monster │  │ EntityConfig      │   │   │
│  │  │ (class) │  │ (class) │  │ (config classes)  │   │   │
│  │  └─────────┘  └─────────┘  └───────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                Turn Management                       │   │
│  │  ┌─────────────┐  ┌────────────────────────────┐   │   │
│  │  │ TurnPhase   │  │ TurnManager                │   │   │
│  │  │   (enum)    │  │ (turn order, phase logic)  │   │   │
│  │  └─────────────┘  └────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │               Future Systems (Phase 3+)              │   │
│  │  Combat | AI | Commands                              │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                              │
│  - Pure C# (no UnityEngine references)                      │
│  - All game rules and logic                                 │
│  - Fully unit testable                                      │
│  - Serializable for networking                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Assembly Structure

### SubGame.Core (noEngineReferences: true)

**Location**: `Assets/_Project/Core/`

**Purpose**: Contains all game logic with zero Unity dependencies.

**Key Principle**: If you accidentally try to use `UnityEngine` in this assembly, it will fail to compile. This enforces the separation.

**Namespaces**:
- `SubGame.Core.Grid` - 3D grid system
- `SubGame.Core.Entities` - Entity interfaces, base class, Submarine, Monster
- `SubGame.Core.Config` - Entity configuration classes
- `SubGame.Core.TurnManagement` - Turn order, phases
- `SubGame.Core.Combat` - Damage, attacks (future)
- `SubGame.Core.AI` - Monster AI (future)
- `SubGame.Core.Commands` - Command pattern actions (future)

### SubGame.Unity

**Location**: `Assets/_Project/Unity/`

**Purpose**: Unity-specific presentation and input handling.

**References**: SubGame.Core

**Namespaces**:
- `SubGame.Unity.Presentation` - Visual representation of game state
- `SubGame.Unity.Input` - Player input handling (future)
- `SubGame.Unity.UI` - UI elements (future)
- `SubGame.Unity.Networking` - Mirror integration (future)

### SubGame.GameManagement

**Location**: `Assets/_Project/GameManagement/`

**Purpose**: High-level game orchestration, scene management.

**References**: SubGame.Core, SubGame.Unity

### SubGame.Tests.EditMode

**Location**: `Assets/Tests/EditMode/`

**Purpose**: Unit tests for Core logic. Run without Unity Play mode.

### SubGame.Tests.PlayMode

**Location**: `Assets/Tests/PlayMode/`

**Purpose**: Integration tests requiring Unity runtime.

---

## Grid System Design

### GridCoordinate

A value type (struct) representing a position in 3D space.

```
GridCoordinate
├── X: int (horizontal)
├── Y: int (vertical/depth in ocean)
├── Z: int (forward)
├── Distance(a, b) → int (Manhattan distance)
├── GetNeighbors() → List<GridCoordinate> (6 adjacent cells)
├── Operators: +, -, ==, !=
└── IEquatable<GridCoordinate> (for dictionary keys)
```

**Design Decisions**:
- **Immutable struct**: Safe for use as dictionary keys, no accidental modification
- **Manhattan distance**: Appropriate for grid-based movement (no diagonals)
- **6 neighbors**: Only orthogonal movement in 3D (±X, ±Y, ±Z)

### IGridSystem

Interface defining grid operations. Allows for different implementations (e.g., infinite grids, chunked grids).

```
IGridSystem
├── Width, Height, Depth: int
├── IsValidCoordinate(coord) → bool
├── IsOccupied(coord) → bool
├── GetValidNeighbors(coord) → List<GridCoordinate>
├── GetCoordinatesInRange(center, range) → List<GridCoordinate>
├── SetOccupied(coord)
├── ClearOccupied(coord)
└── GetAllCoordinates() → IEnumerable<GridCoordinate>
```

### GridSystem

Concrete implementation of IGridSystem for bounded 3D grids.

**Key Features**:
- Bounds checking with clear error messages
- HashSet-based occupancy tracking (O(1) lookup)
- Range queries optimized with bounding box + distance filter

---

## Entity System Design

### IEntity Interface

Interface defining common properties and behaviors for all game entities.

```
IEntity
├── Id: Guid (unique identifier)
├── Name: string (display name)
├── Position: GridCoordinate (current location)
├── Health / MaxHealth: int (hit points)
├── MovementRange: int (cells per turn)
├── AttackRange: int (attack distance)
├── AttackDamage: int (base damage)
├── IsAlive: bool (Health > 0)
├── EntityType: EntityType (Submarine or Monster)
├── TakeDamage(amount) → void
├── Heal(amount) → void
├── SetPosition(newPosition) → void
├── OnDamageTaken: event (entity, damage)
├── OnDeath: event (entity)
└── OnMoved: event (entity, oldPos, newPos)
```

### Entity Base Class

Abstract class implementing IEntity with common functionality.

**Key Features**:
- Constructor validation for all parameters
- Health clamping (0 to MaxHealth)
- Event firing for damage, death, and movement
- Dead entities ignore further damage/healing

### Submarine & Monster Classes

Concrete entity implementations.

```
Submarine (EntityType.Submarine)
├── Default: 100 HP, 3 movement, 2 attack range, 25 damage
└── Faster and more agile than monsters

Monster (EntityType.Monster)
├── Default: 200 HP, 2 movement, 1 attack range, 40 damage
└── Tougher and hits harder than submarines
```

### EntityConfig Classes

Configuration objects for entity stats, enabling data-driven design.

```
EntityConfig (abstract base)
├── Name: string
├── MaxHealth: int
├── MovementRange: int
├── AttackRange: int
├── AttackDamage: int
└── IsValid() → bool

SubmarineConfig : EntityConfig (submarine defaults)
MonsterConfig : EntityConfig (monster defaults)
```

### EntityManager

Central manager for all entities in the game.

```
EntityManager
├── AddEntity(entity) → void (validates position)
├── RemoveEntity(entityId) → bool
├── GetEntity(entityId) → IEntity
├── GetEntityAtPosition(position) → IEntity
├── GetSubmarines() → IEnumerable<IEntity>
├── GetMonsters() → IEnumerable<IEntity>
├── GetLivingEntities() → IEnumerable<IEntity>
├── GetEntitiesInRange(center, range) → IEnumerable<IEntity>
├── IsValidMovePosition(position) → bool
├── OnEntityAdded: event
└── OnEntityRemoved: event
```

**Key Features**:
- Automatically updates grid occupancy on add/remove/move
- Subscribes to entity events for death and movement
- Validates positions against grid bounds and occupancy

---

## Turn Management System Design

### TurnPhase Enum

Defines the phases within each game turn.

```
TurnPhase
├── TurnStart     - Start of turn, before actions
├── PlayerAction  - Submarines take actions
├── EnemyAction   - Monsters take actions
└── TurnEnd       - End of turn, cleanup
```

### TurnManager

Orchestrates turn order and phase progression.

```
TurnManager
├── CurrentTurn: int (1-based turn counter)
├── CurrentPhase: TurnPhase
├── ActiveEntity: IEntity (whose turn it is)
├── HasStarted: bool
├── IsPlayerPhase / IsEnemyPhase: bool
├── StartGame() → void
├── AdvancePhase() → void
├── EndCurrentEntityTurn() → void
├── Reset() → void
├── GetTurnOrder() → IReadOnlyList<IEntity>
├── OnTurnStarted: event (turnNumber)
├── OnTurnEnded: event (turnNumber)
├── OnPhaseChanged: event (newPhase)
└── OnActiveEntityChanged: event (entity)
```

**Turn Flow**:
```
StartGame()
    │
    ▼
┌──────────────┐
│  TurnStart   │ ◄─────────────────────────────┐
└──────┬───────┘                               │
       │ AdvancePhase()                        │
       ▼                                       │
┌──────────────┐                               │
│ PlayerAction │ (cycles through submarines)   │
└──────┬───────┘                               │
       │ AdvancePhase() (no more subs)         │
       ▼                                       │
┌──────────────┐                               │
│ EnemyAction  │ (cycles through monsters)     │
└──────┬───────┘                               │
       │ AdvancePhase() (no more monsters)     │
       ▼                                       │
┌──────────────┐                               │
│   TurnEnd    │                               │
└──────┬───────┘                               │
       │ AdvancePhase()                        │
       └───────────────────────────────────────┘
```

**Key Features**:
- Builds turn order at start of each turn (submarines first, then monsters)
- Skips dead entities automatically
- Events for all state transitions (enables UI updates)

---

## Command System Design

### ICommand Interface

All game actions are encapsulated as command objects for validation, execution, and networking.

```
ICommand
├── Validate(state) → CommandResult
└── Execute(state) → void
```

### CommandResult

Value type representing command outcomes.

```
CommandResult
├── Success: bool
├── ErrorMessage: string (null if success)
├── Ok() → CommandResult (static factory)
└── Fail(message) → CommandResult (static factory)
```

### MoveCommand

Moves an entity to a new position with full validation.

**Validation Checks**:
1. Game has started
2. Entity exists and is alive
3. It's the entity's turn
4. Target is within grid bounds
5. Target is not occupied
6. Target is reachable within movement range (via pathfinding)

### EndTurnCommand

Ends the current entity's turn and advances to next entity/phase.

**Validation Checks**:
1. Game has started
2. Active entity matches command's entity

### AttackCommand

Attacks another entity, dealing damage.

**Validation Checks**:
1. Game has started
2. Attacker exists and is alive
3. It's the attacker's turn
4. Target exists and is alive
5. Not attacking self
6. Target is within attack range (Manhattan distance)

**Execution**:
- Applies attacker's AttackDamage to target's health
- Target death handled automatically via Entity events

---

## GameState Facade

Central facade aggregating all core subsystems.

```
GameState : IGameState
├── Grid: IGridSystem
├── EntityCount: int
├── CurrentTurn: int
├── CurrentPhase: TurnPhase
├── ActiveEntity: IEntity
├── HasStarted: bool
├── IsPlayerPhase: bool
│
├── ExecuteCommand(cmd) → CommandResult
├── AddEntity(entity) / RemoveEntity(id)
├── GetEntity(id) / GetEntityAtPosition(pos)
├── GetSubmarines() / GetMonsters() / GetLivingEntities()
├── MoveEntity(entity, newPos)
├── StartGame() / AdvancePhase() / EndCurrentEntityTurn() / Reset()
├── FindPath(start, end) → IReadOnlyList<GridCoordinate>
├── GetReachablePositions(entity) → IReadOnlyCollection<GridCoordinate>
│
├── Combat:
│   ├── ApplyDamage(target, damage)
│   ├── TryAttack(target) → bool
│   └── GetAttackableTargets() → IEnumerable<IEntity>
│
├── OnEntityMoved: event
├── OnEntityAttacked: event (attacker, target, damage)
├── OnTurnStarted / OnTurnEnded: event
├── OnPhaseChanged: event
├── OnActiveEntityChanged: event
├── OnEntityAdded / OnEntityRemoved: event
└── OnCommandExecuted: event
```

**Key Features**:
- Single entry point for all game operations
- Validates commands before execution
- Aggregates events from subsystems into unified event stream
- Provides pathfinding and reachability queries

---

## Pathfinding System

### Pathfinder

BFS-based pathfinding for 3D grids.

```
Pathfinder
├── FindPath(start, end) → IReadOnlyList<GridCoordinate>
├── GetReachablePositions(start, range) → IReadOnlyCollection<GridCoordinate>
├── PathExists(start, end) → bool
└── GetPathDistance(start, end) → int
```

**Algorithm**: Breadth-First Search (BFS)
- Guarantees shortest path in unweighted grids
- Respects grid bounds and occupied cells
- O(V + E) complexity where V = cells, E = edges

**Movement Rules**:
- 6-directional movement (±X, ±Y, ±Z)
- Cannot move through occupied cells
- Cannot move outside grid bounds

---

## Unity Presentation Layer

### GridVisualizer

MonoBehaviour that renders the grid using Unity Gizmos.

**Responsibilities**:
- Draws grid cells as wireframe cubes
- Draws boundary outline
- Converts between grid coordinates and world positions
- Configurable via Inspector (dimensions, colors, cell size)

**Does NOT**:
- Contain any game logic
- Make decisions about game state
- Store authoritative data

**Coordinate Conversion**:
```
World Position = GridOrigin + (GridCoord * CellSize) + (CellSize / 2)
Grid Coordinate = Floor((WorldPos - GridOrigin) / CellSize)
```

### EntityView (Base Class)

Abstract MonoBehaviour for visual representation of game entities.

```
EntityView : MonoBehaviour
├── EntityId: Guid (links to Core entity)
├── GridPosition: GridCoordinate (current logical position)
├── CellSize: float (for coordinate conversion)
│
├── Initialize(entityId, startPosition, cellSize)
├── UpdatePosition(newPosition) → smooth animation
├── UpdateHealth(currentHealth, maxHealth) → abstract
├── OnDamageTaken(damage) → abstract
├── OnDeath() → abstract
├── SetSelected(selected) → virtual
└── GridToWorldPosition(coord) → Vector3
```

**Key Features**:
- Smooth movement animation using Lerp
- Configurable movement speed
- Base class for submarine/monster-specific visuals

### SubmarineView & MonsterView

Concrete EntityView implementations with entity-specific visuals.

```
SubmarineView : EntityView
├── Material color changes (normal, selected, damaged)
├── Health bar display (shows when damaged)
├── Damage flash effect (0.3s white flash)
└── Death effect (gray color, hide health bar)

MonsterView : EntityView
├── Similar structure to SubmarineView
├── Different color scheme (red/orange)
└── Entity-specific visual effects
```

### GameManager

Central Unity-side orchestrator bridging Core game state with Unity visuals.

```
GameManager : MonoBehaviour
├── Prefabs: submarinePrefab, monsterPrefab
├── Grid Settings: width, height, depth, cellSize
├── GridVisualizer reference
│
├── GameState: GameState (Core)
├── EntityViews: Dictionary<Guid, EntityView>
├── SelectedEntityView: EntityView
│
├── Events:
│   ├── OnGameInitialized
│   ├── OnTurnStarted(turnNumber)
│   ├── OnActiveEntityChanged(entity)
│   └── OnPhaseChanged(phase)
│
├── Entity Management:
│   ├── CreateEntityView(entity) → instantiates prefab
│   └── DestroyEntityView(entityId)
│
├── Command Execution:
│   ├── TryMoveActiveEntity(targetPosition) → bool
│   ├── EndCurrentTurn() → bool
│   └── GetReachablePositions() → IReadOnlyCollection
│
└── Coordinate Conversion:
    ├── GridToWorldPosition(coord) → Vector3
    └── WorldToGridPosition(worldPos) → GridCoordinate
```

**Lifecycle**:
1. Awake: Creates GameState, subscribes to Core events
2. Start: Sets up test entities, starts game
3. Runtime: Responds to Core events, updates EntityViews
4. OnDestroy: Unsubscribes from events

### PlayerInputHandler

Handles player input and converts to game commands.

```
PlayerInputHandler : MonoBehaviour
├── References: gameManager, mainCamera
├── Input Settings: groundLayerMask, endTurnKey (Space)
├── Visual Feedback: movementIndicatorPrefab, colors
│
├── HandleMouseInput()
│   ├── Raycast from camera to ground
│   ├── Convert hit point to GridCoordinate
│   ├── Track hovered cell
│   └── On click: call GameManager.TryMoveActiveEntity()
│
├── HandleKeyboardInput()
│   └── Space: call GameManager.EndCurrentTurn()
│
├── Movement Range Display:
│   ├── ShowMovementRange() → creates indicator objects
│   ├── HideMovementRange()
│   └── ToggleMovementRange()
│
└── Events:
    └── OnActiveEntityChanged → refresh movement range
```

---

## Data Flow

Data flows through the system as follows:

```
Player Input (Unity)
       │
       ▼
PlayerInputHandler (Unity)
       │
       │ Calls GameManager methods
       ▼
GameManager.TryMoveActiveEntity() (Unity)
       │
       │ Creates & executes Command
       ▼
GameState.ExecuteCommand(cmd) (Core)
       │
       │ Validates & Executes
       ▼
GameState publishes Event (Core)
       │
       │ Event subscription
       ▼
GameManager receives Event (Unity)
       │
       │ Updates EntityViews
       ▼
EntityView.UpdatePosition() (Unity)
       │
       │ Smooth animation
       ▼
Visual Update Complete
```

**Key Points**:
- Unity layer calls Core methods via GameManager
- Core publishes events (Observer pattern)
- Unity subscribes to events and updates visuals
- No Unity code in Core, ever
- EntityViews animate smoothly between grid positions

---

## Design Patterns

### Patterns Currently Used

**Value Object (GridCoordinate)**:
- Immutable struct
- Equality based on value, not reference
- Safe for collections and dictionary keys

**Interface Segregation (IGridSystem, IEntity)**:
- Clients depend on interfaces, not implementations
- Allows for future alternative implementations

**Observer Pattern (Entity/TurnManager Events)**:
- Events for state changes (damage, death, movement, turn changes)
- Decouples Core from Unity presentation
- Enables reactive UI updates

**Template Method (Entity base class)**:
- Base class defines common behavior
- Subclasses (Submarine, Monster) provide specific details

**Command Pattern (ICommand, MoveCommand, EndTurnCommand)**:
- All game actions encapsulated as command objects
- Commands validate before execution
- Enables undo/redo, replay, and networking
- Commands are serializable for network transmission

**Facade Pattern (GameState)**:
- Single entry point to all Core subsystems
- Aggregates Grid, EntityManager, TurnManager, Pathfinder
- Provides unified event system for presentation layer

### Patterns Planned for Future Phases

**State Pattern** (Future, if needed):
- Turn phases could become state objects
- Currently handled with enum + switch

---

## Multiplayer Architecture (Future)

The architecture is designed for multiplayer from day one:

**Server-Authoritative**:
- All game logic runs on server (Core assembly)
- Clients send commands, receive events
- No client-side prediction initially (turn-based doesn't need it)

**Deterministic**:
- Same inputs = same outputs
- Important for replay and verification

**Serializable State**:
- `GameState` can serialize entire state
- Commands are serializable data objects
- Events carry minimal data for network efficiency

**Network Integration Points**:
```
Client                          Server
──────                          ──────
PlayerInput
    │
    └──► Command (serialized) ──► GameState.Execute()
                                      │
    ◄── Event (serialized) ◄────────┘
    │
Unity updates visuals
```

---

## Testing Strategy

### Unit Tests (EditMode)

- Test Core logic without Unity
- Fast execution (no Play mode needed)
- High coverage target (90%+)

**Example**:
```csharp
[Test]
public void Distance_AdjacentCoordinates_ReturnsOne()
{
    var a = new GridCoordinate(0, 0, 0);
    var b = new GridCoordinate(1, 0, 0);

    Assert.AreEqual(1, GridCoordinate.Distance(a, b));
}
```

### Integration Tests (PlayMode)

- Test Unity integration
- Require Play mode
- Test event flow, visual updates

### Manual Testing

- Visual verification in Scene view
- Gameplay feel testing
- Camera and UX testing

---

## File Organization

```
Assets/
├── _Project/
│   ├── Core/                    # Pure C# (NO UNITY)
│   │   ├── SubGame.Core.asmdef
│   │   ├── GameState.cs            # Main facade
│   │   ├── Grid/
│   │   │   ├── GridCoordinate.cs
│   │   │   ├── IGridSystem.cs
│   │   │   ├── GridSystem.cs
│   │   │   └── Pathfinder.cs
│   │   ├── Entities/
│   │   │   ├── IEntity.cs
│   │   │   ├── Entity.cs
│   │   │   ├── Submarine.cs
│   │   │   ├── Monster.cs
│   │   │   └── EntityManager.cs
│   │   ├── Config/
│   │   │   └── EntityConfig.cs
│   │   ├── TurnManagement/
│   │   │   ├── TurnPhase.cs
│   │   │   └── TurnManager.cs
│   │   ├── Commands/
│   │   │   ├── ICommand.cs
│   │   │   ├── IGameState.cs
│   │   │   ├── CommandResult.cs
│   │   │   ├── MoveCommand.cs
│   │   │   └── EndTurnCommand.cs
│   │   ├── Combat/             # Future
│   │   └── AI/                 # Future
│   │
│   ├── Unity/                   # Unity-specific
│   │   ├── SubGame.Unity.asmdef
│   │   ├── Presentation/
│   │   │   ├── GridVisualizer.cs
│   │   │   ├── EntityView.cs      # Base class for entity visuals
│   │   │   ├── SubmarineView.cs   # Submarine-specific visuals
│   │   │   └── MonsterView.cs     # Monster-specific visuals
│   │   ├── Input/              # Future (UI-specific input)
│   │   ├── UI/                 # Future
│   │   └── Networking/         # Future
│   │
│   ├── GameManagement/
│   │   ├── SubGame.GameManagement.asmdef
│   │   ├── GameManager.cs         # Unity-side orchestrator
│   │   └── Input/
│   │       └── PlayerInputHandler.cs  # Game input handling
│   │
│   └── Data/
│       ├── Prefabs/
│       └── Scenes/
│           └── GridTest.unity
│
└── Tests/
    ├── EditMode/
    │   ├── SubGame.Tests.EditMode.asmdef
    │   ├── GridCoordinateTests.cs
    │   ├── GridSystemTests.cs
    │   ├── EntityTests.cs
    │   ├── EntityManagerTests.cs
    │   ├── TurnManagerTests.cs
    │   ├── PathfinderTests.cs
    │   ├── CommandTests.cs
    │   └── GameStateTests.cs
    └── PlayMode/
        └── SubGame.Tests.PlayMode.asmdef
```

---

## Coding Conventions

See `docs/STANDARDS.md` for complete coding standards.

**Key Points**:
- Private fields: `_camelCase`
- Public members: `PascalCase`
- Interfaces: `IName`
- XML documentation on all public members
- No magic numbers (use constants)

---

## Version History

- **v1.0** (2025-11-21): Initial architecture document (Phase 1 - Grid System)
- **v1.1** (2025-11-21): Added Entity System and Turn Management (Phase 2)
- **v1.2** (2025-11-21): Added Command Pattern, GameState Facade, and Pathfinding (Phase 3)
- **v1.3** (2025-11-21): Added Unity Presentation Layer - EntityView, SubmarineView, MonsterView, GameManager, PlayerInputHandler (Phase 4)
- **v1.4** (2025-11-22): Added Combat System - AttackCommand, damage application, right-click to attack (Phase 5)

---

## Future Additions

This document will be updated as new systems are implemented:
- Phase 6: AI architecture (monster auto-attack)
- Phase 7: Camera and input system
- Phase 8: Final system integration
