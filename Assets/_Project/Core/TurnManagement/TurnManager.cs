using System;
using System.Collections.Generic;
using SubGame.Core.Entities;

namespace SubGame.Core.TurnManagement
{
    /// <summary>
    /// Manages turn order and phase progression for the game.
    /// </summary>
    public class TurnManager
    {
        private readonly EntityManager _entityManager;
        private readonly List<IEntity> _turnOrder = new List<IEntity>();
        private int _currentEntityIndex;

        /// <summary>
        /// Event fired when a new turn begins.
        /// Parameters: turn number
        /// </summary>
        public event Action<int> OnTurnStarted;

        /// <summary>
        /// Event fired when a turn ends.
        /// Parameters: turn number
        /// </summary>
        public event Action<int> OnTurnEnded;

        /// <summary>
        /// Event fired when the turn phase changes.
        /// Parameters: new phase
        /// </summary>
        public event Action<TurnPhase> OnPhaseChanged;

        /// <summary>
        /// Event fired when the active entity changes.
        /// Parameters: new active entity (can be null at turn boundaries)
        /// </summary>
        public event Action<IEntity> OnActiveEntityChanged;

        /// <summary>
        /// Gets the current turn number (1-based).
        /// </summary>
        public int CurrentTurn { get; private set; }

        /// <summary>
        /// Gets the current phase of the turn.
        /// </summary>
        public TurnPhase CurrentPhase { get; private set; }

        /// <summary>
        /// Gets the currently active entity (the one whose turn it is).
        /// </summary>
        public IEntity ActiveEntity { get; private set; }

        /// <summary>
        /// Gets whether the game has started.
        /// </summary>
        public bool HasStarted => CurrentTurn > 0;

        /// <summary>
        /// Gets whether it's currently the player's action phase.
        /// </summary>
        public bool IsPlayerPhase => CurrentPhase == TurnPhase.PlayerAction;

        /// <summary>
        /// Gets whether it's currently the enemy's action phase.
        /// </summary>
        public bool IsEnemyPhase => CurrentPhase == TurnPhase.EnemyAction;

        /// <summary>
        /// Creates a new TurnManager.
        /// </summary>
        /// <param name="entityManager">The entity manager to use for turn order</param>
        /// <exception cref="ArgumentNullException">Thrown if entityManager is null</exception>
        public TurnManager(EntityManager entityManager)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            CurrentTurn = 0;
            CurrentPhase = TurnPhase.TurnStart;
        }

        /// <summary>
        /// Starts the game and begins the first turn.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if game already started</exception>
        public void StartGame()
        {
            if (HasStarted)
            {
                throw new InvalidOperationException("Game has already started");
            }

            StartNewTurn();
        }

        /// <summary>
        /// Advances to the next phase or turn.
        /// </summary>
        public void AdvancePhase()
        {
            if (!HasStarted)
            {
                throw new InvalidOperationException("Game has not started. Call StartGame() first.");
            }

            switch (CurrentPhase)
            {
                case TurnPhase.TurnStart:
                    TransitionToPhase(TurnPhase.PlayerAction);
                    AdvanceToNextEntity(EntityType.Submarine);
                    break;

                case TurnPhase.PlayerAction:
                    if (!AdvanceToNextEntity(EntityType.Submarine))
                    {
                        // No more submarines to act, move to enemy phase
                        TransitionToPhase(TurnPhase.EnemyAction);
                        if (!AdvanceToNextEntity(EntityType.Monster))
                        {
                            // No monsters either, end turn and start new one
                            TransitionToPhase(TurnPhase.TurnEnd);
                            EndCurrentTurn();
                            StartNewTurn(autoAdvanceToPlayerAction: true);
                        }
                    }
                    break;

                case TurnPhase.EnemyAction:
                    if (!AdvanceToNextEntity(EntityType.Monster))
                    {
                        // No more monsters to act, end the turn and start new one
                        TransitionToPhase(TurnPhase.TurnEnd);
                        EndCurrentTurn();
                        StartNewTurn(autoAdvanceToPlayerAction: true);
                    }
                    break;

                case TurnPhase.TurnEnd:
                    // This case handles manual advancement through TurnEnd if needed
                    EndCurrentTurn();
                    StartNewTurn(autoAdvanceToPlayerAction: true);
                    break;
            }
        }

        /// <summary>
        /// Ends the current entity's turn and advances to the next entity or phase.
        /// </summary>
        public void EndCurrentEntityTurn()
        {
            if (ActiveEntity == null)
            {
                throw new InvalidOperationException("No active entity to end turn for");
            }

            AdvancePhase();
        }

        /// <summary>
        /// Resets the turn manager to initial state.
        /// </summary>
        public void Reset()
        {
            CurrentTurn = 0;
            CurrentPhase = TurnPhase.TurnStart;
            ActiveEntity = null;
            _turnOrder.Clear();
            _currentEntityIndex = -1;
        }

        /// <summary>
        /// Gets the current turn order for display purposes.
        /// </summary>
        /// <returns>Read-only list of entities in turn order</returns>
        public IReadOnlyList<IEntity> GetTurnOrder()
        {
            return _turnOrder.AsReadOnly();
        }

        /// <summary>
        /// Starts a new turn.
        /// </summary>
        /// <param name="autoAdvanceToPlayerAction">If true, automatically advances to PlayerAction phase</param>
        private void StartNewTurn(bool autoAdvanceToPlayerAction = false)
        {
            CurrentTurn++;
            _currentEntityIndex = -1;

            // Build turn order: submarines first, then monsters
            _turnOrder.Clear();
            foreach (var submarine in _entityManager.GetSubmarines())
            {
                if (submarine.IsAlive)
                {
                    _turnOrder.Add(submarine);
                }
            }
            foreach (var monster in _entityManager.GetMonsters())
            {
                if (monster.IsAlive)
                {
                    _turnOrder.Add(monster);
                }
            }

            TransitionToPhase(TurnPhase.TurnStart);
            OnTurnStarted?.Invoke(CurrentTurn);

            // Auto-advance to PlayerAction phase for seamless turn cycling
            if (autoAdvanceToPlayerAction)
            {
                TransitionToPhase(TurnPhase.PlayerAction);
                AdvanceToNextEntity(EntityType.Submarine);
            }
        }

        /// <summary>
        /// Ends the current turn.
        /// </summary>
        private void EndCurrentTurn()
        {
            SetActiveEntity(null);
            OnTurnEnded?.Invoke(CurrentTurn);
        }

        /// <summary>
        /// Transitions to a new phase.
        /// </summary>
        private void TransitionToPhase(TurnPhase newPhase)
        {
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        /// <summary>
        /// Sets the active entity and fires the event.
        /// </summary>
        private void SetActiveEntity(IEntity entity)
        {
            ActiveEntity = entity;
            OnActiveEntityChanged?.Invoke(entity);
        }

        /// <summary>
        /// Advances to the next entity of the specified type in the turn order.
        /// </summary>
        /// <param name="entityType">The type of entity to find</param>
        /// <returns>True if an entity was found and set active, false otherwise</returns>
        private bool AdvanceToNextEntity(EntityType entityType)
        {
            for (int i = _currentEntityIndex + 1; i < _turnOrder.Count; i++)
            {
                var entity = _turnOrder[i];
                if (entity.EntityType == entityType && entity.IsAlive)
                {
                    _currentEntityIndex = i;
                    SetActiveEntity(entity);
                    return true;
                }
            }

            // No more entities of this type
            SetActiveEntity(null);
            return false;
        }
    }
}
