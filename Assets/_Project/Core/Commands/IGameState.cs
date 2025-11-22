using System;
using System.Collections.Generic;
using SubGame.Core.Entities;
using SubGame.Core.Grid;
using SubGame.Core.TurnManagement;

namespace SubGame.Core.Commands
{
    /// <summary>
    /// Interface for game state access.
    /// Provides a unified API for commands to query and modify game state.
    /// </summary>
    public interface IGameState
    {
        #region Grid Access

        /// <summary>
        /// Gets the grid system for coordinate validation and pathfinding.
        /// </summary>
        IGridSystem Grid { get; }

        #endregion

        #region Entity Access

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        IEntity GetEntity(Guid entityId);

        /// <summary>
        /// Gets the entity at a specific position.
        /// </summary>
        IEntity GetEntityAtPosition(GridCoordinate position);

        /// <summary>
        /// Gets all submarines.
        /// </summary>
        IEnumerable<IEntity> GetSubmarines();

        /// <summary>
        /// Gets all monsters.
        /// </summary>
        IEnumerable<IEntity> GetMonsters();

        /// <summary>
        /// Gets all living entities.
        /// </summary>
        IEnumerable<IEntity> GetLivingEntities();

        /// <summary>
        /// Checks if a position is valid for movement (in bounds and unoccupied).
        /// Only checks single cell - use CanEntityMoveTo for multi-tile entities.
        /// </summary>
        bool IsValidMovePosition(GridCoordinate position);

        /// <summary>
        /// Checks if an entity can move to a position (all cells must be valid).
        /// Accounts for entity size and allows overlap with entity's current position.
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <param name="position">Target anchor position</param>
        /// <returns>True if the entity can fit at the position</returns>
        bool CanEntityMoveTo(IEntity entity, GridCoordinate position);

        #endregion

        #region Turn Access

        /// <summary>
        /// Gets the current turn number.
        /// </summary>
        int CurrentTurn { get; }

        /// <summary>
        /// Gets the current turn phase.
        /// </summary>
        TurnPhase CurrentPhase { get; }

        /// <summary>
        /// Gets the currently active entity (whose turn it is).
        /// </summary>
        IEntity ActiveEntity { get; }

        /// <summary>
        /// Whether the game has started.
        /// </summary>
        bool HasStarted { get; }

        /// <summary>
        /// Whether it's currently the player's action phase.
        /// </summary>
        bool IsPlayerPhase { get; }

        #endregion

        #region Actions

        /// <summary>
        /// Moves an entity to a new position.
        /// Does not validate - use IsValidMovePosition first.
        /// </summary>
        void MoveEntity(IEntity entity, GridCoordinate newPosition);

        /// <summary>
        /// Applies damage to an entity.
        /// </summary>
        /// <param name="target">The entity to damage</param>
        /// <param name="damage">Amount of damage to apply</param>
        void ApplyDamage(IEntity target, int damage);

        /// <summary>
        /// Advances to the next phase or entity turn.
        /// </summary>
        void AdvancePhase();

        /// <summary>
        /// Ends the current entity's turn.
        /// </summary>
        void EndCurrentEntityTurn();

        #endregion

        #region Pathfinding

        /// <summary>
        /// Finds a path from start to end position.
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">Target position</param>
        /// <returns>List of coordinates forming the path, or empty if no path exists</returns>
        IReadOnlyList<GridCoordinate> FindPath(GridCoordinate start, GridCoordinate end);

        /// <summary>
        /// Gets all positions reachable by an entity within its movement range.
        /// </summary>
        /// <param name="entity">The entity to check movement for</param>
        /// <returns>Set of reachable positions</returns>
        IReadOnlyCollection<GridCoordinate> GetReachablePositions(IEntity entity);

        /// <summary>
        /// Calculates the minimum distance between two entities, accounting for their sizes.
        /// Returns the shortest Manhattan distance between any cell of entity A and any cell of entity B.
        /// </summary>
        int GetDistanceBetweenEntities(IEntity entityA, IEntity entityB);

        #endregion
    }
}
