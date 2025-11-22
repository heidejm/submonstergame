using System;
using System.Collections.Generic;
using SubGame.Core.Commands;
using SubGame.Core.Entities;
using SubGame.Core.Grid;
using SubGame.Core.TurnManagement;

namespace SubGame.Core
{
    /// <summary>
    /// Main facade for the game state.
    /// Aggregates all core subsystems and provides a unified API.
    /// </summary>
    public class GameState : IGameState
    {
        private readonly GridSystem _gridSystem;
        private readonly EntityManager _entityManager;
        private readonly TurnManager _turnManager;
        private readonly Pathfinder _pathfinder;

        #region Events

        /// <summary>
        /// Fired when an entity moves.
        /// </summary>
        public event Action<IEntity, GridCoordinate, GridCoordinate> OnEntityMoved;

        /// <summary>
        /// Fired when a new turn starts.
        /// </summary>
        public event Action<int> OnTurnStarted;

        /// <summary>
        /// Fired when a turn ends.
        /// </summary>
        public event Action<int> OnTurnEnded;

        /// <summary>
        /// Fired when the turn phase changes.
        /// </summary>
        public event Action<TurnPhase> OnPhaseChanged;

        /// <summary>
        /// Fired when the active entity changes.
        /// </summary>
        public event Action<IEntity> OnActiveEntityChanged;

        /// <summary>
        /// Fired when an entity is added.
        /// </summary>
        public event Action<IEntity> OnEntityAdded;

        /// <summary>
        /// Fired when an entity is removed.
        /// </summary>
        public event Action<IEntity> OnEntityRemoved;

        /// <summary>
        /// Fired when a command is executed.
        /// </summary>
        public event Action<ICommand> OnCommandExecuted;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public IGridSystem Grid => _gridSystem;

        /// <inheritdoc/>
        public int CurrentTurn => _turnManager.CurrentTurn;

        /// <inheritdoc/>
        public TurnPhase CurrentPhase => _turnManager.CurrentPhase;

        /// <inheritdoc/>
        public IEntity ActiveEntity => _turnManager.ActiveEntity;

        /// <inheritdoc/>
        public bool HasStarted => _turnManager.HasStarted;

        /// <inheritdoc/>
        public bool IsPlayerPhase => _turnManager.IsPlayerPhase;

        /// <summary>
        /// Gets the number of entities in the game.
        /// </summary>
        public int EntityCount => _entityManager.Count;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new game state with the specified grid dimensions.
        /// </summary>
        /// <param name="gridWidth">Width of the grid</param>
        /// <param name="gridHeight">Height of the grid (vertical/depth)</param>
        /// <param name="gridDepth">Depth of the grid (forward)</param>
        public GameState(int gridWidth, int gridHeight, int gridDepth)
        {
            _gridSystem = new GridSystem(gridWidth, gridHeight, gridDepth);
            _entityManager = new EntityManager(_gridSystem);
            _turnManager = new TurnManager(_entityManager);
            _pathfinder = new Pathfinder(_gridSystem);

            // Wire up internal events to external events
            _turnManager.OnTurnStarted += turn => OnTurnStarted?.Invoke(turn);
            _turnManager.OnTurnEnded += turn => OnTurnEnded?.Invoke(turn);
            _turnManager.OnPhaseChanged += phase => OnPhaseChanged?.Invoke(phase);
            _turnManager.OnActiveEntityChanged += entity => OnActiveEntityChanged?.Invoke(entity);
            _entityManager.OnEntityAdded += entity => OnEntityAdded?.Invoke(entity);
            _entityManager.OnEntityRemoved += entity => OnEntityRemoved?.Invoke(entity);
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Executes a command if it's valid.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns>Result indicating success or failure</returns>
        public CommandResult ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                return CommandResult.Fail("Command cannot be null");
            }

            var validationResult = command.Validate(this);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            command.Execute(this);
            OnCommandExecuted?.Invoke(command);

            return CommandResult.Ok();
        }

        #endregion

        #region Entity Management

        /// <summary>
        /// Adds an entity to the game.
        /// </summary>
        public void AddEntity(IEntity entity)
        {
            _entityManager.AddEntity(entity);
        }

        /// <summary>
        /// Removes an entity from the game.
        /// </summary>
        public bool RemoveEntity(Guid entityId)
        {
            return _entityManager.RemoveEntity(entityId);
        }

        /// <inheritdoc/>
        public IEntity GetEntity(Guid entityId)
        {
            return _entityManager.GetEntity(entityId);
        }

        /// <inheritdoc/>
        public IEntity GetEntityAtPosition(GridCoordinate position)
        {
            return _entityManager.GetEntityAtPosition(position);
        }

        /// <inheritdoc/>
        public IEnumerable<IEntity> GetSubmarines()
        {
            return _entityManager.GetSubmarines();
        }

        /// <inheritdoc/>
        public IEnumerable<IEntity> GetMonsters()
        {
            return _entityManager.GetMonsters();
        }

        /// <inheritdoc/>
        public IEnumerable<IEntity> GetLivingEntities()
        {
            return _entityManager.GetLivingEntities();
        }

        /// <inheritdoc/>
        public bool IsValidMovePosition(GridCoordinate position)
        {
            return _entityManager.IsValidMovePosition(position);
        }

        #endregion

        #region Movement

        /// <inheritdoc/>
        public void MoveEntity(IEntity entity, GridCoordinate newPosition)
        {
            var oldPosition = entity.Position;
            entity.SetPosition(newPosition);
            OnEntityMoved?.Invoke(entity, oldPosition, newPosition);
        }

        #endregion

        #region Combat

        /// <summary>
        /// Event fired when an entity attacks another.
        /// Parameters: attacker, target, damage dealt
        /// </summary>
        public event Action<IEntity, IEntity, int> OnEntityAttacked;

        /// <inheritdoc/>
        public void ApplyDamage(IEntity target, int damage)
        {
            target.TakeDamage(damage);
        }

        /// <summary>
        /// Attempts to attack a target entity with the active entity.
        /// </summary>
        /// <param name="target">The entity to attack</param>
        /// <returns>True if attack was executed</returns>
        public bool TryAttack(IEntity target)
        {
            if (ActiveEntity == null || target == null)
            {
                return false;
            }

            var command = new AttackCommand(ActiveEntity, target);
            var result = ExecuteCommand(command);

            if (result.Success)
            {
                OnEntityAttacked?.Invoke(ActiveEntity, target, ActiveEntity.AttackDamage);
            }

            return result.Success;
        }

        /// <summary>
        /// Gets entities within attack range of the active entity.
        /// </summary>
        /// <returns>Collection of attackable entities</returns>
        public IEnumerable<IEntity> GetAttackableTargets()
        {
            if (ActiveEntity == null)
            {
                yield break;
            }

            foreach (var entity in GetLivingEntities())
            {
                if (entity.Id == ActiveEntity.Id)
                {
                    continue;
                }

                int distance = GridCoordinate.Distance(ActiveEntity.Position, entity.Position);
                if (distance <= ActiveEntity.AttackRange)
                {
                    yield return entity;
                }
            }
        }

        #endregion

        #region Turn Management

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame()
        {
            _turnManager.StartGame();
        }

        /// <inheritdoc/>
        public void AdvancePhase()
        {
            _turnManager.AdvancePhase();
        }

        /// <inheritdoc/>
        public void EndCurrentEntityTurn()
        {
            _turnManager.EndCurrentEntityTurn();
        }

        /// <summary>
        /// Resets the game state.
        /// </summary>
        public void Reset()
        {
            _turnManager.Reset();
            _entityManager.Clear();
        }

        #endregion

        #region Pathfinding

        /// <inheritdoc/>
        public IReadOnlyList<GridCoordinate> FindPath(GridCoordinate start, GridCoordinate end)
        {
            return _pathfinder.FindPath(start, end);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<GridCoordinate> GetReachablePositions(IEntity entity)
        {
            if (entity == null || !entity.IsAlive)
            {
                return new HashSet<GridCoordinate>();
            }

            return _pathfinder.GetReachablePositions(entity.Position, entity.MovementRange);
        }

        /// <summary>
        /// Gets the path distance between two positions.
        /// </summary>
        /// <returns>Distance, or -1 if no path exists</returns>
        public int GetPathDistance(GridCoordinate start, GridCoordinate end)
        {
            return _pathfinder.GetPathDistance(start, end);
        }

        #endregion
    }
}
