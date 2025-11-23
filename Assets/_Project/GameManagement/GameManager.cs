using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SubGame.Core;
using SubGame.Core.AI;
using SubGame.Core.Commands;
using SubGame.Core.Entities;
using SubGame.Core.Grid;
using SubGame.Core.TurnManagement;
using SubGame.Unity.Presentation;

namespace SubGame.GameManagement
{
    /// <summary>
    /// Main Unity-side orchestrator for the game.
    /// Bridges Core game state with Unity visuals and input.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _submarinePrefab;
        [SerializeField] private GameObject _monsterPrefab;

        [Header("Grid Settings")]
        [SerializeField] private int _gridWidth = 15;
        [SerializeField] private int _gridHeight = 15;
        [SerializeField] private int _gridDepth = 15;
        [SerializeField] private float _cellSize = 2f; // Larger cells for better visibility

        [Header("References")]
        [SerializeField] private GridVisualizer _gridVisualizer;
        [SerializeField] private RuntimeGridRenderer _runtimeGridRenderer;

        private GameState _gameState;
        private Dictionary<Guid, EntityView> _entityViews = new Dictionary<Guid, EntityView>();
        private EntityView _selectedEntityView;
        private MonsterAIController _aiController;

        // Store the path for the current move (calculated before move executes)
        private IReadOnlyList<GridCoordinate> _pendingMovePath;

        [Header("AI Settings")]
        [SerializeField] private float _aiTurnDelay = 1.0f; // Delay before AI acts

        #region Events

        /// <summary>
        /// Fired when the game state is initialized.
        /// </summary>
        public event Action OnGameInitialized;

        /// <summary>
        /// Fired when a turn starts.
        /// </summary>
        public event Action<int> OnTurnStarted;

        /// <summary>
        /// Fired when the active entity changes.
        /// </summary>
        public event Action<IEntity> OnActiveEntityChanged;

        /// <summary>
        /// Fired when the turn phase changes.
        /// </summary>
        public event Action<TurnPhase> OnPhaseChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current game state.
        /// </summary>
        public GameState GameState => _gameState;

        /// <summary>
        /// Gets the currently active entity view.
        /// </summary>
        public EntityView SelectedEntityView => _selectedEntityView;

        /// <summary>
        /// Gets the cell size for coordinate conversions.
        /// </summary>
        public float CellSize => _cellSize;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeGameState();
        }

        private void Start()
        {
            SetupTestEntities();
            StartGame();
            Debug.Log($"Game started. ActiveEntity: {_gameState?.ActiveEntity?.Name ?? "null"}, Phase: {_gameState?.CurrentPhase}");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void InitializeGameState()
        {
            _gameState = new GameState(_gridWidth, _gridHeight, _gridDepth);

            // Initialize AI controller with default aggressive deck
            _aiController = new MonsterAIController();
            _aiController.DefaultDeck = MonsterAIController.CreateDefaultAggressiveDeck();
            _aiController.OnCardDrawn += HandleAICardDrawn;
            _aiController.OnTurnComplete += HandleAITurnComplete;

            SubscribeToEvents();

            // Update grid visualizers if available
            if (_gridVisualizer != null)
            {
                _gridVisualizer.SetDimensions(_gridWidth, _gridHeight, _gridDepth);
            }

            if (_runtimeGridRenderer != null)
            {
                _runtimeGridRenderer.SetDimensions(_gridWidth, _gridHeight, _gridDepth, _cellSize);
            }

            OnGameInitialized?.Invoke();
        }

        private void SubscribeToEvents()
        {
            _gameState.OnEntityAdded += HandleEntityAdded;
            _gameState.OnEntityRemoved += HandleEntityRemoved;
            _gameState.OnEntityMoved += HandleEntityMoved;
            _gameState.OnTurnStarted += HandleTurnStarted;
            _gameState.OnTurnEnded += HandleTurnEnded;
            _gameState.OnPhaseChanged += HandlePhaseChanged;
            _gameState.OnActiveEntityChanged += HandleActiveEntityChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_gameState != null)
            {
                _gameState.OnEntityAdded -= HandleEntityAdded;
                _gameState.OnEntityRemoved -= HandleEntityRemoved;
                _gameState.OnEntityMoved -= HandleEntityMoved;
                _gameState.OnTurnStarted -= HandleTurnStarted;
                _gameState.OnTurnEnded -= HandleTurnEnded;
                _gameState.OnPhaseChanged -= HandlePhaseChanged;
                _gameState.OnActiveEntityChanged -= HandleActiveEntityChanged;
            }
        }

        /// <summary>
        /// Sets up test entities for development.
        /// </summary>
        private void SetupTestEntities()
        {
            // Add a submarine - positioned in lower area of grid
            var submarine = new Submarine(new GridCoordinate(5, 2, 5), "Player Sub");
            _gameState.AddEntity(submarine);

            // Add a monster - positioned further away for tactical distance
            var monster = new Monster(new GridCoordinate(10, 2, 10), "Sea Beast");
            _gameState.AddEntity(monster);
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame()
        {
            _gameState.StartGame();
            _gameState.AdvancePhase(); // Move to PlayerAction phase
        }

        #endregion

        #region Entity Management

        private void HandleEntityAdded(IEntity entity)
        {
            CreateEntityView(entity);
        }

        private void HandleEntityRemoved(IEntity entity)
        {
            DestroyEntityView(entity.Id);
        }

        private void CreateEntityView(IEntity entity)
        {
            GameObject prefab = entity.EntityType == EntityType.Submarine
                ? _submarinePrefab
                : _monsterPrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"No prefab assigned for entity type {entity.EntityType}");
                return;
            }

            Vector3 worldPos = GridToWorldPosition(entity.Position);
            GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);
            instance.name = $"{entity.EntityType}_{entity.Name}";

            EntityView view = instance.GetComponent<EntityView>();
            if (view != null)
            {
                // Pass entity size for proper scaling of multi-tile entities
                view.Initialize(entity.Id, entity.Position, _cellSize, entity.Size);
                view.UpdateHealth(entity.Health, entity.MaxHealth);
                _entityViews[entity.Id] = view;

                // Subscribe to entity events
                entity.OnDamageTaken += (e, damage) =>
                {
                    view.OnDamageTaken(damage);
                    view.UpdateHealth(e.Health, e.MaxHealth);
                };
                entity.OnDeath += e => view.OnDeath();
            }
            else
            {
                Debug.LogError($"Prefab for {entity.EntityType} is missing EntityView component");
                Destroy(instance);
            }
        }

        private void DestroyEntityView(Guid entityId)
        {
            if (_entityViews.TryGetValue(entityId, out EntityView view))
            {
                _entityViews.Remove(entityId);
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void HandleEntityMoved(IEntity entity, GridCoordinate oldPos, GridCoordinate newPos)
        {
            if (_entityViews.TryGetValue(entity.Id, out EntityView view))
            {
                // Use the pre-calculated path (calculated before move executed)
                var path = _pendingMovePath;
                _pendingMovePath = null; // Clear after use

                if (path != null && path.Count > 1)
                {
                    Debug.Log($"Moving along path with {path.Count} waypoints");
                    view.MoveAlongPath(path);
                }
                else
                {
                    // Fallback to direct movement if no path found
                    Debug.Log("No path found, using direct movement");
                    view.UpdatePosition(newPos);
                }
            }
        }

        private void HandleTurnStarted(int turnNumber)
        {
            Debug.Log($"Turn {turnNumber} started");
            OnTurnStarted?.Invoke(turnNumber);
        }

        private void HandleTurnEnded(int turnNumber)
        {
            Debug.Log($"Turn {turnNumber} ended");
        }

        private void HandlePhaseChanged(TurnPhase phase)
        {
            Debug.Log($"Phase changed to {phase}");
            OnPhaseChanged?.Invoke(phase);
        }

        private void HandleActiveEntityChanged(IEntity entity)
        {
            // Deselect previous
            if (_selectedEntityView != null)
            {
                _selectedEntityView.SetSelected(false);
            }

            // Select new
            if (entity != null && _entityViews.TryGetValue(entity.Id, out EntityView view))
            {
                _selectedEntityView = view;
                _selectedEntityView.SetSelected(true);
            }
            else
            {
                _selectedEntityView = null;
            }

            OnActiveEntityChanged?.Invoke(entity);

            // If it's a monster's turn, trigger AI
            if (entity != null && entity.EntityType == EntityType.Monster)
            {
                StartCoroutine(ExecuteAITurnWithDelay(entity));
            }
        }

        private IEnumerator ExecuteAITurnWithDelay(IEntity monster)
        {
            // Wait for visual feedback
            yield return new WaitForSeconds(_aiTurnDelay);

            // Execute AI turn
            _aiController.ExecuteTurn(monster, _gameState);
        }

        private void HandleAICardDrawn(IEntity monster, BehaviorCard card)
        {
            Debug.Log($"[AI] {monster.Name} drew card: {card.Name}");
        }

        private void HandleAITurnComplete(IEntity monster)
        {
            Debug.Log($"[AI] {monster.Name} completed turn");

            // End the monster's turn after a short delay for animations
            StartCoroutine(EndAITurnWithDelay());
        }

        private IEnumerator EndAITurnWithDelay()
        {
            // Wait for movement animations to complete
            yield return new WaitForSeconds(0.5f);

            // Advance to next entity
            _gameState.EndCurrentEntityTurn();
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Attempts to move the active entity to a position.
        /// </summary>
        /// <param name="targetPosition">Target grid position</param>
        /// <returns>True if move was executed</returns>
        public bool TryMoveActiveEntity(GridCoordinate targetPosition)
        {
            if (_gameState == null)
            {
                Debug.LogError("GameState is null! Was GameManager.Awake() called?");
                return false;
            }

            if (_gameState.ActiveEntity == null)
            {
                Debug.Log($"No active entity to move. HasStarted={_gameState.HasStarted}, Phase={_gameState.CurrentPhase}");
                return false;
            }

            // Calculate path BEFORE executing the move (so destination isn't occupied yet)
            // Use entity-aware pathfinding for multi-tile entities
            _pendingMovePath = _gameState.FindPathForEntity(_gameState.ActiveEntity, targetPosition);

            var command = new MoveCommand(_gameState.ActiveEntity, targetPosition);
            var result = _gameState.ExecuteCommand(command);

            if (!result.Success)
            {
                Debug.Log($"Move failed: {result.ErrorMessage}");
                _pendingMovePath = null;
            }

            return result.Success;
        }

        /// <summary>
        /// Ends the current entity's turn.
        /// </summary>
        /// <returns>True if turn was ended</returns>
        public bool EndCurrentTurn()
        {
            if (_gameState.ActiveEntity == null)
            {
                Debug.Log("No active entity");
                return false;
            }

            var command = new EndTurnCommand(_gameState.ActiveEntity);
            var result = _gameState.ExecuteCommand(command);

            if (!result.Success)
            {
                Debug.Log($"End turn failed: {result.ErrorMessage}");
            }

            return result.Success;
        }

        /// <summary>
        /// Gets reachable positions for the active entity.
        /// </summary>
        public IReadOnlyCollection<GridCoordinate> GetReachablePositions()
        {
            if (_gameState.ActiveEntity == null)
            {
                return new HashSet<GridCoordinate>();
            }

            return _gameState.GetReachablePositions(_gameState.ActiveEntity);
        }

        /// <summary>
        /// Gets the path from the active entity to a target position.
        /// Uses entity-aware pathfinding for multi-tile entities.
        /// </summary>
        /// <param name="targetPosition">Target grid position</param>
        /// <returns>List of coordinates forming the path, or empty if no path exists</returns>
        public IReadOnlyList<GridCoordinate> GetPathTo(GridCoordinate targetPosition)
        {
            if (_gameState.ActiveEntity == null)
            {
                return new List<GridCoordinate>();
            }

            return _gameState.FindPathForEntity(_gameState.ActiveEntity, targetPosition);
        }

        /// <summary>
        /// Attempts to attack an entity at a position.
        /// </summary>
        /// <param name="targetPosition">Position of the target</param>
        /// <returns>True if attack was executed</returns>
        public bool TryAttackAtPosition(GridCoordinate targetPosition)
        {
            if (_gameState == null)
            {
                Debug.LogError("GameState is null!");
                return false;
            }

            if (_gameState.ActiveEntity == null)
            {
                Debug.Log("No active entity to attack with");
                return false;
            }

            var target = _gameState.GetEntityAtPosition(targetPosition);
            if (target == null)
            {
                Debug.Log("No target at position");
                return false;
            }

            bool success = _gameState.TryAttack(target);
            if (!success)
            {
                Debug.Log("Attack failed");
            }
            else
            {
                Debug.Log($"{_gameState.ActiveEntity.Name} attacked {target.Name} for {_gameState.ActiveEntity.AttackDamage} damage!");
            }

            return success;
        }

        /// <summary>
        /// Gets entities that can be attacked by the active entity.
        /// </summary>
        public IEnumerable<IEntity> GetAttackableTargets()
        {
            if (_gameState == null)
            {
                return System.Array.Empty<IEntity>();
            }

            return _gameState.GetAttackableTargets();
        }

        #endregion

        #region Coordinate Conversion

        /// <summary>
        /// Converts grid coordinate to world position.
        /// </summary>
        public Vector3 GridToWorldPosition(GridCoordinate coord)
        {
            return new Vector3(
                coord.X * _cellSize + _cellSize / 2f,
                coord.Y * _cellSize + _cellSize / 2f,
                coord.Z * _cellSize + _cellSize / 2f
            );
        }

        /// <summary>
        /// Converts world position to grid coordinate.
        /// </summary>
        public GridCoordinate WorldToGridPosition(Vector3 worldPos)
        {
            return new GridCoordinate(
                Mathf.FloorToInt(worldPos.x / _cellSize),
                Mathf.FloorToInt(worldPos.y / _cellSize),
                Mathf.FloorToInt(worldPos.z / _cellSize)
            );
        }

        #endregion
    }
}
