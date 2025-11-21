using System;
using SubGame.Core.Entities;

namespace SubGame.Core.Commands
{
    /// <summary>
    /// Command to end the current entity's turn.
    /// Advances to the next entity or phase.
    /// </summary>
    public class EndTurnCommand : ICommand
    {
        private readonly Guid _entityId;

        /// <summary>
        /// Gets the ID of the entity ending their turn.
        /// </summary>
        public Guid EntityId => _entityId;

        /// <summary>
        /// Creates a new end turn command.
        /// </summary>
        /// <param name="entityId">ID of the entity ending their turn</param>
        public EndTurnCommand(Guid entityId)
        {
            _entityId = entityId;
        }

        /// <summary>
        /// Creates a new end turn command for an entity.
        /// </summary>
        /// <param name="entity">The entity ending their turn</param>
        public EndTurnCommand(IEntity entity)
            : this(entity?.Id ?? Guid.Empty)
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

            // Check there is an active entity
            if (state.ActiveEntity == null)
            {
                return CommandResult.Fail("No active entity");
            }

            // Check it's this entity's turn
            if (state.ActiveEntity.Id != _entityId)
            {
                return CommandResult.Fail("It is not this entity's turn");
            }

            return CommandResult.Ok();
        }

        /// <inheritdoc/>
        public void Execute(IGameState state)
        {
            state.EndCurrentEntityTurn();
        }
    }
}
