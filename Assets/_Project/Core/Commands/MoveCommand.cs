using System;
using System.Linq;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Core.Commands
{
    /// <summary>
    /// Command to move an entity to a new position.
    /// Validates movement range, pathfinding, and turn state.
    /// </summary>
    public class MoveCommand : ICommand
    {
        private readonly Guid _entityId;
        private readonly GridCoordinate _targetPosition;

        /// <summary>
        /// Gets the ID of the entity to move.
        /// </summary>
        public Guid EntityId => _entityId;

        /// <summary>
        /// Gets the target position.
        /// </summary>
        public GridCoordinate TargetPosition => _targetPosition;

        /// <summary>
        /// Creates a new move command.
        /// </summary>
        /// <param name="entityId">ID of the entity to move</param>
        /// <param name="targetPosition">Position to move to</param>
        public MoveCommand(Guid entityId, GridCoordinate targetPosition)
        {
            _entityId = entityId;
            _targetPosition = targetPosition;
        }

        /// <summary>
        /// Creates a new move command for an entity.
        /// </summary>
        /// <param name="entity">The entity to move</param>
        /// <param name="targetPosition">Position to move to</param>
        public MoveCommand(IEntity entity, GridCoordinate targetPosition)
            : this(entity?.Id ?? Guid.Empty, targetPosition)
        {
        }

        /// <inheritdoc/>
        public CommandResult Validate(IGameState state)
        {
            // Check game has started
            if (!state.HasStarted)
            {
                return CommandResult.Fail("Game has not started");
            }

            // Get the entity
            var entity = state.GetEntity(_entityId);
            if (entity == null)
            {
                return CommandResult.Fail("Entity not found");
            }

            // Check entity is alive
            if (!entity.IsAlive)
            {
                return CommandResult.Fail("Entity is dead");
            }

            // Check it's this entity's turn
            if (state.ActiveEntity == null || state.ActiveEntity.Id != _entityId)
            {
                return CommandResult.Fail("It is not this entity's turn");
            }

            // Check target position is valid
            if (!state.Grid.IsValidCoordinate(_targetPosition))
            {
                return CommandResult.Fail("Target position is outside grid bounds");
            }

            // Check if already at target (before occupancy check since entity occupies its own position)
            if (entity.Position.Equals(_targetPosition))
            {
                return CommandResult.Fail("Entity is already at target position");
            }

            // Check target is not occupied
            if (!state.IsValidMovePosition(_targetPosition))
            {
                return CommandResult.Fail("Target position is occupied");
            }

            // Check target is within movement range (using pathfinding)
            var reachablePositions = state.GetReachablePositions(entity);
            if (!reachablePositions.Contains(_targetPosition))
            {
                return CommandResult.Fail("Target position is not reachable within movement range");
            }

            return CommandResult.Ok();
        }

        /// <inheritdoc/>
        public void Execute(IGameState state)
        {
            var entity = state.GetEntity(_entityId);
            state.MoveEntity(entity, _targetPosition);
        }
    }
}
