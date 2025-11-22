using System;
using System.Collections.Generic;
using UnityEngine;
using SubGame.Core;
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
        [SerializeField] private int _gridWidth = 10;
        [SerializeField] private int _gridHeight = 5;
        [SerializeField] private int _gridDepth = 10;
        [SerializeField] private float _cellSize = 1f;

        [Header("References")]
        [SerializeField] private GridVisualizer _gridVisualizer;

        private GameState _gameState;
        private Dictionary<Guid, EntityView> _entityViews = new Dictionary<Guid, EntityView>();
        private EntityView _selectedEntityView;

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
            SubscribeToEvents();

            // Update grid visualizer if available
            if (_gridVisualizer != null)
            {
                _gridVisualizer.SetDimensions(_gridWidth, _gridHeight, _gridDepth);
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
            // Add a submarine
            var submarine = new Submarine(new GridCoordinate(2, 0, 2), "Player Sub");
            _gameState.AddEntity(submarine);

            // Add a monster
            var monster = new Monster(new GridCoordinate(7, 0, 7), "Sea Beast");
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
                view.Initialize(entity.Id, entity.Position, _cellSize);
                view.UpdateHealth(entity.Health, entity.MaxHealth);
                _entityViews[entity.Id] = view;

                // Subscribe to entity events
                entity.OnDamageTaken += (e, damage) => view.OnDamageTaken(damage);
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
                view.UpdatePosition(newPos);
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

            var command = new MoveCommand(_gameState.ActiveEntity, targetPosition);
            var result = _gameState.ExecuteCommand(command);

            if (!result.Success)
            {
                Debug.Log($"Move failed: {result.ErrorMessage}");
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
