# Architecture Guide - Submarine Monster Tactical Game

**Version**: 1.0
**Last Updated**: 2025-11-21

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
│  │               Future Systems (Phase 2+)              │   │
│  │  Entities | Combat | TurnManagement | AI | Commands  │   │
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
- `SubGame.Core.Entities` - Submarines, monsters (future)
- `SubGame.Core.Combat` - Damage, attacks (future)
- `SubGame.Core.TurnManagement` - Turn order, phases (future)
- `SubGame.Core.AI` - Monster AI (future)
- `SubGame.Core.Commands` - Command pattern actions (future)
- `SubGame.Core.Config` - Game constants, balance data (future)

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

---

## Data Flow (Future)

When fully implemented, data will flow as follows:

```
Player Input (Unity)
       │
       ▼
PlayerInputHandler (Unity)
       │
       │ Creates Command
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
       │ Updates visuals
       ▼
EntityView.UpdatePosition() (Unity)
```

**Key Points**:
- Unity layer calls Core methods
- Core publishes events (Observer pattern)
- Unity subscribes to events and updates visuals
- No Unity code in Core, ever

---

## Design Patterns

### Patterns Currently Used

**Value Object (GridCoordinate)**:
- Immutable struct
- Equality based on value, not reference
- Safe for collections and dictionary keys

**Interface Segregation (IGridSystem)**:
- Clients depend on interface, not implementation
- Allows for future alternative implementations

### Patterns Planned for Future Phases

**Command Pattern** (Phase 3):
- All game actions as command objects
- Enables undo, replay, networking
- `MoveCommand`, `AttackCommand`, etc.

**Observer Pattern** (Phase 4):
- Events for state changes
- Decouples Core from Unity presentation

**State Pattern** (Phase 2):
- Turn phases as state objects
- Clean phase transitions

**Facade Pattern** (Phase 3):
- `GameState` as single entry point to Core
- Hides subsystem complexity

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
│   │   ├── Grid/
│   │   │   ├── GridCoordinate.cs
│   │   │   ├── IGridSystem.cs
│   │   │   └── GridSystem.cs
│   │   ├── Entities/           # Future
│   │   ├── Combat/             # Future
│   │   ├── TurnManagement/     # Future
│   │   ├── AI/                 # Future
│   │   ├── Commands/           # Future
│   │   └── Config/             # Future
│   │
│   ├── Unity/                   # Unity-specific
│   │   ├── SubGame.Unity.asmdef
│   │   ├── Presentation/
│   │   │   └── GridVisualizer.cs
│   │   ├── Input/              # Future
│   │   ├── UI/                 # Future
│   │   └── Networking/         # Future
│   │
│   ├── GameManagement/
│   │   └── SubGame.GameManagement.asmdef
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
    │   └── GridSystemTests.cs
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

- **v1.0** (2025-11-21): Initial architecture document (Phase 1)

---

## Future Additions

This document will be updated as new systems are implemented:
- Phase 2: Entity system architecture
- Phase 3: Command pattern details
- Phase 4: Event system architecture
- Phase 5: Combat system design
- Phase 6: AI architecture
- Phase 7: Camera and input system
- Phase 8: Final system integration
