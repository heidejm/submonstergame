# Implementation Plan - Submarine Monster Tactical Game

**Version**: 1.0
**Last Updated**: 2025-11-21
**Target**: Minimal Playable Prototype (v0.1.0)

---

## Executive Summary

This plan breaks down the minimal viable prototype into 8 sequential phases. Each phase delivers a testable vertical slice while maintaining strict adherence to STANDARDS.md principles. The architecture prioritizes separation of concerns, multiplayer-readiness, and maintainability from day one.

---

## Technology Stack

### Core Technologies
- **Engine**: Unity 2022.3 LTS (Long Term Support)
- **Language**: C# (.NET Standard 2.1)
- **Version Control**: Git
- **IDE**: Visual Studio Community 2022 OR VS Code

### Unity Packages (All Free)
- **Test Framework**: Built-in unit and integration testing
- **Input System**: Modern input handling (multiplayer-ready)
- **Cinemachine**: Advanced camera controls
- **ProBuilder**: Prototype geometry creation
- **TextMeshPro**: Modern text rendering
- **Mirror Networking**: Future multiplayer support (architecture ready from start)
- **DOTween (Free)**: Optional animation tweening

### Development Tools
- **Linting/Formatting**: EditorConfig + Roslyn Analyzers
- **Testing**: Unity Test Framework (NUnit)
- **CI/CD**: GitHub Actions (optional, setup later)

---

## Game Scope - Minimal Prototype (v0.1.0)

### What We're Building

A playable prototype that proves the core concept:
- **3D Grid Battlefield**: True 3-axis (X/Y/Z) grid movement in ocean environment
- **1-4 Submarines**: Player-controlled, turn-based
- **1 Monster**: AI-controlled ocean creature
- **Turn-Based Combat**: Move, attack, strategic positioning
- **Free Camera**: Omniscient view with orbit, pan, zoom controls
- **Basic Combat**: Damage, health, entity destruction
- **Win/Lose Conditions**: Victory when monster destroyed, defeat when all subs destroyed

### What We're NOT Building (Yet)

- Settlement/crafting systems (Kingdom Death meta-game)
- Campaign progression and timeline
- Multiple monster types or advanced AI behaviors
- Multiplayer networking (architecture supports it, not implemented)
- Polished graphics, sound, or visual effects
- Extensive balancing and tuning
- Gear/equipment systems

---

## Core Architecture

### High-Level Design

```
┌─────────────────────────────────────────────────────────────┐
│                    UNITY PRESENTATION LAYER                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ GridVisual   │  │ SubmarineView│  │ MonsterView  │      │
│  │ izer         │  │              │  │              │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
│         │                 │                  │              │
│  ┌──────▼──────────────────▼──────────────────▼────────┐   │
│  │          GameManager (Unity MonoBehaviour)          │   │
│  │  - Orchestrates Unity-specific lifecycle           │   │
│  │  - Bridges Input to Core Logic                     │   │
│  │  - Syncs Core State to Visuals                     │   │
│  └──────────────────────┬──────────────────────────────┘   │
└─────────────────────────┼──────────────────────────────────┘
                          │
                          │ Calls methods, subscribes to events
                          │
┌─────────────────────────▼──────────────────────────────────┐
│                    CORE GAME LOGIC (Pure C#)               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              GameState (Facade)                      │  │
│  │  - Entry point for all game logic                   │  │
│  │  - Coordinates subsystems                           │  │
│  │  - Publishes events for state changes               │  │
│  └───┬──────────────────────────────────────────────┬───┘  │
│      │                                              │      │
│  ┌───▼─────────┐  ┌─────────────┐  ┌──────────▼────────┐  │
│  │ TurnManager │  │ GridSystem  │  │ CombatResolver    │  │
│  │ - Turn order│  │ - 3D grid   │  │ - Damage calc     │  │
│  │ - Phases    │  │ - Pathfinding│ │ - Hit detection   │  │
│  └─────────────┘  └─────────────┘  └───────────────────┘  │
│                                                            │
│  ┌─────────────┐  ┌─────────────┐  ┌───────────────────┐  │
│  │ Submarine   │  │ Monster     │  │ MonsterAI         │  │
│  │ (Entity)    │  │ (Entity)    │  │ - Decision tree   │  │
│  └─────────────┘  └─────────────┘  └───────────────────┘  │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            Command Pattern (Actions)                 │  │
│  │  MoveCommand, AttackCommand, EndTurnCommand          │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────┘
```

### Key Architectural Principles

**Separation of Concerns**:
- **Core**: Pure C# game logic, zero Unity dependencies, fully testable
- **Unity Presentation**: Visualization only, subscribes to Core events
- **GameState Facade**: Single entry point for all game operations

**Design Patterns**:
- **Command Pattern**: All player/AI actions (Move, Attack, EndTurn)
- **Observer Pattern**: Events for state changes (Entity moved, attacked, died)
- **State Pattern**: Turn phases and game states
- **Facade Pattern**: GameState hides subsystem complexity
- **Factory Pattern**: Entity creation

**Multiplayer-Ready Architecture**:
- Server-authoritative design (all logic in Core can run on server)
- Deterministic (same inputs = same outputs)
- Commands are serializable
- GameState can serialize/deserialize entire state
- Events broadcast state changes (easy to network)
- **No Core logic changes needed when adding multiplayer**

---

## Project Structure

```
SubmarineMonsterGame/
├── .git/
├── .gitignore
├── .editorconfig
├── STANDARDS.md              # Development standards
├── IMPLEMENTATION_PLAN.md    # This document
├── ARCHITECTURE.md           # Detailed system design (create in Phase 1)
├── README.md                 # Setup and usage
├── CHANGELOG.md              # Version history
├── Assets/
│   ├── _Project/             # All custom content
│   │   ├── Core/             # Pure C# game logic (NO Unity dependencies)
│   │   │   ├── Core.asmdef   # Assembly definition (noEngineReferences: true)
│   │   │   ├── Grid/
│   │   │   │   ├── GridCoordinate.cs
│   │   │   │   ├── GridSystem.cs
│   │   │   │   ├── IGridSystem.cs
│   │   │   ├── Entities/
│   │   │   │   ├── IEntity.cs
│   │   │   │   ├── Entity.cs (base)
│   │   │   │   ├── Submarine.cs
│   │   │   │   ├── Monster.cs
│   │   │   │   ├── EntityManager.cs
│   │   │   ├── Combat/
│   │   │   │   ├── ICombatResolver.cs
│   │   │   │   ├── CombatResolver.cs
│   │   │   │   ├── CombatResult.cs
│   │   │   ├── TurnManagement/
│   │   │   │   ├── TurnManager.cs
│   │   │   │   ├── TurnPhase.cs
│   │   │   ├── AI/
│   │   │   │   ├── IAIController.cs
│   │   │   │   ├── MonsterAI.cs
│   │   │   ├── Commands/
│   │   │   │   ├── ICommand.cs
│   │   │   │   ├── MoveCommand.cs
│   │   │   │   ├── AttackCommand.cs
│   │   │   │   ├── EndTurnCommand.cs
│   │   │   ├── Config/
│   │   │   │   ├── GameConstants.cs
│   │   │   │   ├── SubmarineConfig.cs
│   │   │   │   ├── MonsterConfig.cs
│   │   │   └── GameState.cs  # Main facade
│   │   ├── Unity/            # Unity-specific presentation
│   │   │   ├── UnityPresentation.asmdef
│   │   │   ├── Presentation/
│   │   │   │   ├── GridVisualizer.cs
│   │   │   │   ├── EntityView.cs (base)
│   │   │   │   ├── SubmarineView.cs
│   │   │   │   ├── MonsterView.cs
│   │   │   ├── Input/
│   │   │   │   ├── PlayerInputHandler.cs
│   │   │   │   ├── CameraController.cs
│   │   │   ├── UI/
│   │   │   │   ├── TurnIndicatorUI.cs
│   │   │   │   ├── EntityInfoPanel.cs
│   │   │   │   ├── HealthBarUI.cs
│   │   │   └── Networking/   # Future multiplayer
│   │   │       ├── NetworkGameManager.cs
│   │   ├── GameManagement/
│   │   │   ├── GameManager.cs       # Main Unity entry point
│   │   │   ├── SceneController.cs
│   │   └── Data/
│   │       ├── Prefabs/
│   │       │   ├── SubmarinePrefab.prefab
│   │       │   ├── MonsterPrefab.prefab
│   │       └── Scenes/
│   │           ├── MainMenu.unity
│   │           ├── GameplayPrototype.unity
│   │           └── TestScenes/
│   ├── Tests/                # Separate assembly
│   │   ├── EditMode/         # Unit tests (no Unity runtime)
│   │   │   ├── Tests.EditMode.asmdef
│   │   │   ├── Core/
│   │   │   │   ├── GridCoordinateTests.cs
│   │   │   │   ├── GridSystemTests.cs
│   │   │   │   ├── SubmarineTests.cs
│   │   │   │   ├── TurnManagerTests.cs
│   │   │   │   ├── MoveCommandTests.cs
│   │   │   │   ├── AttackCommandTests.cs
│   │   │   │   ├── CombatResolverTests.cs
│   │   │   │   ├── MonsterAITests.cs
│   │   └── PlayMode/         # Integration tests
│   │       ├── SubmarineMovementTests.cs
│   │       ├── CombatIntegrationTests.cs
│   └── ThirdParty/           # External assets
│       ├── Mirror/
│       ├── DOTween/
├── Packages/
│   └── manifest.json
└── ProjectSettings/
```

---

## Implementation Phases

### Phase 0: Development Environment Setup

**Goal**: Install and configure all development tools.

**Deliverables**:
- [ ] Unity 2022.3 LTS installed
- [ ] IDE configured (VS Community 2022 or VS Code)
- [ ] Git repository initialized
- [ ] `.gitignore` configured for Unity
- [ ] `.editorconfig` created for C# formatting
- [ ] Unity packages installed (Cinemachine, ProBuilder, TextMeshPro, Mirror)
- [ ] Roslyn Analyzers enabled
- [ ] Pre-commit hooks configured

**Implementation Steps**:

1. **Install Unity Hub and Unity 2022.3 LTS**
   - Download Unity Hub
   - Install Unity 2022.3 LTS with modules:
     - Windows/Mac/Linux Build Support
     - Documentation
     - Visual Studio Community (optional)

2. **IDE Setup** (choose one):
   - **Option A**: Visual Studio Community 2022
     - Install with ".NET desktop development" and "Game development with Unity" workloads
   - **Option B**: VS Code
     - Install extensions: C#, C# Dev Kit, Unity Code Snippets, Unity Tools

3. **Git Configuration**:
   ```bash
   git init
   git config user.name "Your Name"
   git config user.email "your.email@example.com"
   ```

4. **Create `.gitignore`** (use Unity template from GitHub)

5. **Create `.editorconfig`**:
   ```ini
   root = true

   [*.cs]
   indent_style = space
   indent_size = 4
   end_of_line = crlf
   charset = utf-8
   trim_trailing_whitespace = true
   insert_final_newline = true
   max_line_length = 100
   ```

6. **Install Unity Packages** (via Package Manager):
   - Test Framework
   - Input System
   - Cinemachine
   - ProBuilder
   - TextMeshPro
   - Mirror (from GitHub or Asset Store)

7. **Enable Roslyn Analyzers**:
   - Edit > Project Settings > Editor
   - Check "Roslyn Analyzers"

8. **Set up Git Hooks** (optional but recommended):
   - Create `.git/hooks/pre-commit` script
   - Prevent commits of secret files
   - Check for debug statements

---

### Phase 1: Foundation & Grid System

**Goal**: Establish project structure and implement 3D grid logic.

**Deliverables**:
- [ ] Unity project created with folder structure
- [ ] Assembly definitions configured (Core, UnityPresentation, Tests)
- [ ] ARCHITECTURE.md written
- [ ] README.md updated
- [ ] `GridCoordinate` struct (X, Y, Z coordinates)
- [ ] `GridSystem` class (3D grid, bounds checking, occupancy tracking)
- [ ] `GridVisualizer` MonoBehaviour (debug visualization in Unity)
- [ ] Unit tests for grid logic (90%+ coverage target)
- [ ] Test scene with visible 3D grid

**Key Components**:

**GridCoordinate.cs**:
```csharp
public struct GridCoordinate : IEquatable<GridCoordinate>
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public static readonly GridCoordinate Zero = new GridCoordinate(0, 0, 0);

    public static int Distance(GridCoordinate a, GridCoordinate b)
    {
        // Manhattan distance in 3D
    }

    public List<GridCoordinate> GetNeighbors()
    {
        // Returns 6 neighbors (±X, ±Y, ±Z)
    }
}
```

**GridSystem.cs**:
```csharp
public class GridSystem : IGridSystem
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _depth;
    private readonly Dictionary<GridCoordinate, IEntity> _occupants;

    public GridSystem(int width, int height, int depth) { }

    public bool IsValidCoordinate(GridCoordinate coord) { }
    public bool IsOccupied(GridCoordinate coord) { }
    public IEntity GetEntityAt(GridCoordinate coord) { }
    public List<GridCoordinate> GetNeighbors(GridCoordinate coord) { }
}
```

**Success Criteria**:
- Unit tests pass with 90%+ coverage
- Grid visible in Unity scene from all angles
- Can identify coordinates visually
- ARCHITECTURE.md explains grid design

---

### Phase 2: Entity System & Turn Manager

**Goal**: Implement core entity logic and turn-based sequencing.

**Deliverables**:
- [ ] `IEntity` interface
- [ ] `Entity` base class
- [ ] `Submarine` class (stats, health, movement/attack range)
- [ ] `Monster` class
- [ ] `EntityManager` (tracks all entities)
- [ ] `TurnManager` (turn order, phase transitions)
- [ ] `TurnPhase` state machine
- [ ] Config classes (`SubmarineConfig`, `MonsterConfig`)
- [ ] Unit tests (80%+ coverage target)
- [ ] Integration test scene

**Key Components**:

**IEntity.cs**:
```csharp
public interface IEntity
{
    Guid Id { get; }
    GridCoordinate Position { get; }
    int Health { get; }
    int MaxHealth { get; }
    int MovementRange { get; }
    int AttackRange { get; }
    int AttackDamage { get; }
    bool IsAlive { get; }

    void TakeDamage(int amount);
    void Move(GridCoordinate newPosition);
}
```

**TurnManager.cs**:
```csharp
public class TurnManager
{
    private List<IEntity> _turnOrder;
    private int _currentIndex;

    public IEntity CurrentEntity { get; }
    public event Action<TurnChangedEvent> OnTurnChanged;

    public void InitializeTurnOrder(List<IEntity> entities) { }
    public void NextTurn() { }
    public bool IsEntityTurn(Guid entityId) { }
}
```

**Success Criteria**:
- Entities can be created, take damage, die
- Turn manager cycles through entities correctly
- Tests validate edge cases

---

### Phase 3: Command Pattern & Movement

**Goal**: Implement action system using Command pattern with submarine movement.

**Deliverables**:
- [ ] `ICommand` interface
- [ ] `MoveCommand` with validation and pathfinding
- [ ] `EndTurnCommand`
- [ ] `GameState` facade (aggregates all subsystems)
- [ ] Basic pathfinding (BFS in 3D grid)
- [ ] Command validation system
- [ ] Unit tests
- [ ] Test scene with movable submarine

**Key Components**:

**ICommand.cs**:
```csharp
public interface ICommand
{
    bool Validate(GameState state);
    void Execute(GameState state);
    string GetValidationError();
}
```

**GameState.cs** (Facade):
```csharp
public class GameState
{
    private readonly GridSystem _grid;
    private readonly EntityManager _entityManager;
    private readonly TurnManager _turnManager;
    private readonly CombatResolver _combatResolver;

    // Events
    public event Action<EntityMovedEvent> OnEntityMoved;
    public event Action<EntityAttackedEvent> OnEntityAttacked;
    public event Action<TurnChangedEvent> OnTurnChanged;

    // Command execution
    public bool ExecuteCommand(ICommand command)
    {
        if (!command.Validate(this))
            return false;

        command.Execute(this);
        return true;
    }

    // Query methods
    public IEntity GetEntity(Guid id) => _entityManager.GetEntity(id);
    public bool IsOccupied(GridCoordinate coord) => _grid.IsOccupied(coord);
}
```

**MoveCommand.cs**:
```csharp
public class MoveCommand : ICommand
{
    private readonly Guid _entityId;
    private readonly GridCoordinate _destination;

    public bool Validate(GameState state)
    {
        // Check: entity exists, is entity's turn, within movement range, path valid
    }

    public void Execute(GameState state)
    {
        // Move entity, update grid, publish event
    }
}
```

**Pathfinding** (in GridSystem):
```csharp
public List<GridCoordinate> FindPath(GridCoordinate start, GridCoordinate end, int maxDistance)
{
    // BFS (Breadth-First Search) for 3D grid
    // Returns null if no valid path or exceeds maxDistance
}
```

**Success Criteria**:
- Submarine can move within range on 3D grid
- Movement validation prevents invalid moves
- Tests cover all validation cases
- Manual test in Unity scene works

---

### Phase 4: Unity Presentation Layer

**Goal**: Build Unity presentation layer that visualizes game state.

**Deliverables**:
- [ ] `GameManager` MonoBehaviour (orchestrates Unity side)
- [ ] `EntityView` base class
- [ ] `SubmarineView` and `MonsterView` (visual representation)
- [ ] `PlayerInputHandler` (mouse input to commands)
- [ ] Event subscription system (Core → Unity)
- [ ] Smooth movement animations
- [ ] Placeholder submarine/monster models (ProBuilder)
- [ ] Main gameplay scene

**Key Components**:

**EntityView.cs** (Base):
```csharp
public abstract class EntityView : MonoBehaviour
{
    protected Guid _entityId;

    public void Initialize(Guid entityId)
    {
        _entityId = entityId;
    }

    public abstract void UpdatePosition(GridCoordinate coord);
    public abstract void UpdateHealth(int current, int max);
}
```

**GameManager.cs**:
```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _submarinePrefab;
    [SerializeField] private GameObject _monsterPrefab;

    private GameState _gameState;
    private Dictionary<Guid, EntityView> _entityViews;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Create Core GameState
        _gameState = new GameState(...);

        // Subscribe to events
        _gameState.OnEntityMoved += HandleEntityMoved;
        _gameState.OnEntityAttacked += HandleEntityAttacked;

        // Spawn entities
        SpawnSubmarine(...);
        SpawnMonster(...);
    }

    private void HandleEntityMoved(EntityMovedEvent evt)
    {
        _entityViews[evt.EntityId].UpdatePosition(evt.NewPosition);
    }
}
```

**PlayerInputHandler.cs**:
```csharp
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        // Raycast to get world position
        // Convert to grid coordinate
        // Call GameManager to attempt move
    }
}
```

**Success Criteria**:
- Can click grid cells to move submarine
- Visual feedback is smooth
- Clear separation: GameManager calls Core, Core publishes events, Unity updates visuals
- No Unity code in Core assembly

---

### Phase 5: Combat System

**Goal**: Implement combat mechanics with damage and health.

**Deliverables**:
- [ ] `ICombatResolver` interface
- [ ] `CombatResolver` (damage calculation)
- [ ] `AttackCommand`
- [ ] `CombatResult` data class
- [ ] Range checking for attacks
- [ ] Health UI display (health bars)
- [ ] Visual feedback for attacks
- [ ] Unit tests for combat

**Key Components**:

**CombatResolver.cs**:
```csharp
public class CombatResolver : ICombatResolver
{
    public CombatResult ResolveAttack(IEntity attacker, IEntity defender)
    {
        int damage = CalculateDamage(attacker, defender);
        defender.TakeDamage(damage);

        return new CombatResult
        {
            AttackerId = attacker.Id,
            DefenderId = defender.Id,
            Damage = damage,
            DefenderDestroyed = !defender.IsAlive
        };
    }

    private int CalculateDamage(IEntity attacker, IEntity defender)
    {
        // Simple: Use attacker's AttackDamage stat
        // Future: Add randomness, armor, critical hits
        return attacker.AttackDamage;
    }
}
```

**AttackCommand.cs**:
```csharp
public class AttackCommand : ICommand
{
    private readonly Guid _attackerId;
    private readonly Guid _defenderId;

    public bool Validate(GameState state)
    {
        // Check: entities exist, attacker's turn, within attack range
    }

    public void Execute(GameState state)
    {
        // Resolve combat, publish event, handle entity death
    }
}
```

**Success Criteria**:
- Submarine can attack monster (and vice versa)
- Health decreases visually
- Entity is destroyed and removed when health reaches 0
- Combat tests validate all edge cases

---

### Phase 6: Monster AI

**Goal**: Implement basic AI so monsters can act autonomously.

**Deliverables**:
- [ ] `IAIController` interface
- [ ] `MonsterAI` with decision tree
- [ ] AI integration with TurnManager
- [ ] AI can move towards target and attack when in range
- [ ] Unit tests for AI logic (with mocked GameState)
- [ ] Turn indicator UI
- [ ] AI behavior observable in game

**Key Components**:

**IAIController.cs**:
```csharp
public interface IAIController
{
    ICommand DecideAction(IEntity entity, GameState state);
}
```

**MonsterAI.cs**:
```csharp
public class MonsterAI : IAIController
{
    public ICommand DecideAction(IEntity entity, GameState state)
    {
        var monster = entity as Monster;
        var submarines = state.EntityManager.GetSubmarines();

        if (submarines.Count == 0)
            return new EndTurnCommand(monster.Id);

        // Find nearest submarine
        var target = FindNearestTarget(monster, submarines);

        // Check if in attack range
        int distance = GridCoordinate.Distance(monster.Position, target.Position);

        if (distance <= monster.AttackRange)
        {
            return new AttackCommand(monster.Id, target.Id);
        }
        else
        {
            // Move towards target
            var destination = GetMoveTowardsTarget(monster, target, state.Grid);
            return new MoveCommand(monster.Id, destination);
        }
    }
}
```

**GameState Integration**:
```csharp
public void ProcessAITurn()
{
    var currentEntity = _turnManager.CurrentEntity;

    if (currentEntity is Monster)
    {
        var command = _aiController.DecideAction(currentEntity, this);
        ExecuteCommand(command);
        EndTurn();
    }
}
```

**Success Criteria**:
- Monster autonomously moves towards submarines
- Monster attacks when in range
- Turns cycle correctly between player and AI
- AI behavior is predictable and testable

---

### Phase 7: Camera & UX Polish

**Goal**: Implement free 3D camera controls and improve user experience.

**Deliverables**:
- [ ] `CameraController` with orbit, pan, zoom
- [ ] Grid cell highlighting on hover
- [ ] Valid move indicators (highlight reachable cells)
- [ ] Attack range visualization
- [ ] UI improvements (turn order, entity selection panel)
- [ ] Keyboard shortcuts
- [ ] Polished input handling

**Key Components**:

**CameraController.cs**:
```csharp
public class CameraController : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float _orbitSpeed = 100f;
    [SerializeField] private float _minPolarAngle = 10f;
    [SerializeField] private float _maxPolarAngle = 80f;

    [Header("Pan Settings")]
    [SerializeField] private float _panSpeed = 10f;

    [Header("Zoom Settings")]
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _minDistance = 5f;
    [SerializeField] private float _maxDistance = 50f;

    private Vector3 _pivotPoint;
    private float _currentDistance;
    private float _currentAzimuth;  // Horizontal angle
    private float _currentPolar;    // Vertical angle

    private void Update()
    {
        HandleOrbit();   // Left-click drag
        HandlePan();     // Right-click drag
        HandleZoom();    // Scroll wheel
        UpdateCameraPosition();
    }
}
```

**Alternative**: Use Cinemachine Virtual Camera with Orbital Transposer (simpler but less custom control)

**GridCellHighlighter.cs**:
```csharp
public class GridCellHighlighter : MonoBehaviour
{
    public void HighlightCell(GridCoordinate coord) { }
    public void ShowValidMoves(List<GridCoordinate> coords) { }
    public void ShowAttackRange(List<GridCoordinate> coords) { }
    public void ClearHighlights() { }
}
```

**UI Enhancements**:
- Turn order display (list of entities)
- Entity selection panel (stats, health, ranges)
- Action buttons (Move, Attack, End Turn)
- Keyboard shortcuts (Spacebar: End Turn, Tab: Cycle subs, 1-4: Select sub)

**Success Criteria**:
- Camera is intuitive and allows viewing from all angles
- Grid cells highlight clearly on hover
- Valid moves are visually obvious
- UI provides necessary information at a glance
- Game is playable without documentation

---

### Phase 8: Integration, Testing & Documentation

**Goal**: Final integration, comprehensive testing, complete documentation.

**Deliverables**:
- [ ] Full integration testing (manual test suite)
- [ ] Performance profiling
- [ ] Bug fixes
- [ ] Win/lose conditions implemented
- [ ] Complete ARCHITECTURE.md
- [ ] Complete README.md with screenshots
- [ ] CHANGELOG.md finalized
- [ ] ROADMAP.md for future features
- [ ] v0.1.0 release tagged

**Implementation Steps**:

1. **Integration Testing**:
   - Play through multiple full games
   - Test edge cases:
     - All submarines destroyed (lose condition)
     - All monsters destroyed (win condition)
     - Submarine and monster destroy each other simultaneously
     - Movement blocked in all directions
   - Create manual test checklist

2. **Performance Profiling**:
   - Use Unity Profiler (Window > Analysis > Profiler)
   - Check CPU usage during pathfinding, entity updates, UI updates
   - Check memory (no leaks after extended play)
   - Verify 60 FPS on dev machine

3. **Bug Fixes**:
   - Fix critical bugs (crashes, game-breaking)
   - Log minor issues for future (GitHub issues)

4. **Win/Lose Conditions**:
   - Implement game end detection
   - Show UI screen when game ends
   - "Play Again" button to restart

5. **Complete Documentation**:
   - ARCHITECTURE.md: Final diagrams, system interactions, design decisions
   - README.md: Screenshots, setup instructions, controls, "How to Play"
   - CHANGELOG.md: Summary of all phases, version 0.1.0 features
   - ROADMAP.md: Future enhancements (multiplayer, settlement, campaign)

6. **Final Commit & Tag**:
   - Commit: `chore: Finalize v0.1.0 minimal prototype`
   - Git tag: `v0.1.0`

**Success Criteria**:
- Game is fully playable from start to finish
- No critical bugs
- Documentation is complete and accurate
- Project structure is clean and maintainable
- Ready to show to others for feedback
- All automated tests pass

---

## Testing Strategy

### Unit Testing

**Framework**: Unity Test Framework (NUnit)

**Structure**:
- EditMode tests: `Assets/Tests/EditMode/` (for pure C# logic)
- PlayMode tests: `Assets/Tests/PlayMode/` (for Unity integration)

**Coverage Goals**:
- Core systems: 90%+ coverage
- Commands: 100% coverage (critical for correctness)
- AI: 80%+ coverage
- Unity presentation: Manual testing primarily

**Test Naming Convention**:
```csharp
[Test]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    var grid = new GridSystem(10, 10, 10);

    // Act
    bool result = grid.IsValidCoordinate(new GridCoordinate(5, 5, 5));

    // Assert
    Assert.IsTrue(result);
}
```

### Integration Testing

**PlayMode Tests** (require Unity runtime):
- Full command execution flow
- GameManager initialization
- Event propagation from Core to Unity

**Example**:
```csharp
[UnityTest]
public IEnumerator Submarine_MovesTo_TargetPosition()
{
    // Arrange: Create GameManager
    // Act: Execute move command
    // Wait for animation
    // Assert: Verify position
}
```

### Manual Testing Checklist

**Phase 1**: Grid visibility, coordinate accuracy, bounds checking
**Phase 2**: Entity creation, turn order, health system
**Phase 3**: Movement validation, pathfinding
**Phase 4**: Click-to-move, smooth animations
**Phase 5**: Attack damage, health bars, entity destruction
**Phase 6**: AI turn taking, logical decisions
**Phase 7**: Camera controls, UI clarity, input responsiveness
**Phase 8**: Full game playthrough, win/lose conditions

---

## Tooling & Automation

### C# Formatting & Linting

**EditorConfig** (`.editorconfig` at root):
- Enforces consistent style across IDEs
- Configured in Phase 0

**Roslyn Analyzers**:
- Enable in Unity: Edit > Project Settings > Editor > "Roslyn Analyzers"
- Provides real-time code analysis

**Command Line Formatting** (for CI/CD):
```bash
dotnet format --verify-no-changes
```

### Git Hooks

**Pre-commit Hook** (`.git/hooks/pre-commit`):
- Prevent commits of secret files (`.env`, `credentials.*`)
- Check for debugging code (`Debug.Log`)
- Run formatter (if dotnet SDK installed)

**Pre-push Hook**:
- Reminder to run tests before pushing

### Unity Editor Extensions

**Custom Tools** (create as needed):
- Grid Gizmo Tool: Visualize grid in Scene view
- Entity Spawner: Quick spawn for testing
- Game State Inspector: Debug current turn, entity states

### CI/CD (Optional)

**GitHub Actions** (setup in later phases):
- `.github/workflows/unity-test.yml`
- Run tests on push/PR
- Requires Unity license activation (free Personal license)

---

## Risk Mitigation

### Risk 1: 3D Grid Navigation UX is Confusing

**Mitigation**:
- Prototype early (Phase 1-2): Test grid visualization before gameplay
- User feedback in Phase 4
- Visual aids: Grid lines, hover highlights, depth cueing
- Multiple camera angles

**Contingency**:
- Simplify to 2D grid with elevation (like XCOM) if 3D is too confusing
- Decision point: End of Phase 4

### Risk 2: Camera Controls are Clunky

**Mitigation**:
- Reference existing games (XCOM, Civilization)
- Use Cinemachine (proven solution)
- Configurable sensitivity
- Entire phase (7) dedicated to camera polish

**Contingency**:
- Use fixed camera angles (preset views) instead of free orbit
- Add "reset camera" button

### Risk 3: Pathfinding Performance Issues

**Mitigation**:
- Profile early (Phase 3)
- Keep grid small initially (10×10×10 = 1000 cells)
- Limit pathfinding calls (only on input, not per frame)

**Contingency**:
- Optimize algorithm (A* with octile distance)
- Pre-compute paths or use jump-point search
- Cache frequently-used paths

### Risk 4: Unity/C# Separation is Too Complex

**Mitigation**:
- Follow STANDARDS.md strictly
- Assembly definitions enforce separation
- Regular code reviews

**Contingency**:
- Allow limited Unity references in Core with STANDARDS-EXCEPTION comments
- Re-evaluate after Phase 4
- Pragmatism over purity (but document)

### Risk 5: Monster AI is Too Dumb or Too Smart

**Mitigation**:
- Start simple (Phase 6: basic AI only)
- Tunable parameters
- Unit tests verify logic
- Playtesting for feel

**Contingency**:
- Add more sophisticated decision tree (cover, health checks)
- Add randomness or "mistakes" if too smart

### Risk 6: Scope Creep

**Mitigation**:
- Strict phase adherence
- Document ideas in ROADMAP.md for post-v0.1
- Constantly ask: "Does this prove the core concept?"

**Contingency**:
- Review progress regularly
- Cut features from later phases if needed

---

## Future Roadmap (Post-v0.1.0)

### Phase 9: Multiplayer Integration
- Integrate Mirror networking
- Implement NetworkGameManager
- Synchronize game state across clients
- Test co-op gameplay (multiple players controlling subs)

### Phase 10: Expanded Content
- Multiple monster types with unique behaviors
- More submarine variants with different stats
- Varied battlefield sizes and configurations
- Special abilities and powers

### Phase 11: Kingdom Death Meta-Game (Settlement Phase)
- Settlement management between battles
- Resource collection from defeated monsters
- Crafting system for submarine upgrades
- Facility construction and upgrades
- Random events

### Phase 12: Campaign Mode
- Lantern year progression
- Difficulty scaling
- Story elements and objectives
- Save/load system
- Permadeath mechanics

### Phase 13: Polish & Release Preparation
- Professional art assets
- Sound effects and music
- Tutorial and onboarding
- Balancing and tuning
- Platform-specific optimizations

---

## Success Criteria for v0.1.0

- [x] Can play a full game (1-4 submarines vs 1 monster)
- [x] Win/lose conditions work correctly
- [x] 3D grid navigation is clear and usable
- [x] Camera controls are intuitive
- [x] AI provides appropriate challenge
- [x] No critical bugs
- [x] Code adheres to STANDARDS.md
- [x] ~80% test coverage on core systems
- [x] Documentation is complete and accurate
- [x] Ready to demonstrate and gather feedback

---

## Appendix: Quick Reference

### Key File Locations

- **Standards**: `STANDARDS.md`
- **Architecture**: `ARCHITECTURE.md`
- **Plan**: `IMPLEMENTATION_PLAN.md` (this document)
- **Changelog**: `CHANGELOG.md`
- **Core Logic**: `Assets/_Project/Core/`
- **Unity Code**: `Assets/_Project/Unity/`
- **Tests**: `Assets/Tests/`

### Git Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Example**:
```
feat(combat): Add critical hit system

Implements 5% base crit chance with 2x damage multiplier.

Closes #42
```

### Common Unity Shortcuts

- **F**: Frame selected object in Scene view
- **Ctrl+D**: Duplicate selected object
- **Ctrl+Shift+F**: Align Scene view to selected object
- **Space**: Hand tool (pan Scene view)

### Testing Commands

- Run tests: Window > General > Test Runner
- EditMode: Fast, no Unity runtime
- PlayMode: Requires Unity runtime, slower

---

**End of Implementation Plan**
