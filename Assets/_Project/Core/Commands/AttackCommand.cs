using System;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Core.Commands
{
    /// <summary>
    /// Command to attack another entity.
    /// Validates attack range and turn state, then applies damage.
    /// </summary>
    public class AttackCommand : ICommand
    {
        private readonly Guid _attackerId;
        private readonly Guid _targetId;

        /// <summary>
        /// Gets the ID of the attacking entity.
        /// </summary>
        public Guid AttackerId => _attackerId;

        /// <summary>
        /// Gets the ID of the target entity.
        /// </summary>
        public Guid TargetId => _targetId;

        /// <summary>
        /// Creates a new attack command.
        /// </summary>
        /// <param name="attackerId">ID of the attacking entity</param>
        /// <param name="targetId">ID of the target entity</param>
        public AttackCommand(Guid attackerId, Guid targetId)
        {
            _attackerId = attackerId;
            _targetId = targetId;
        }

        /// <summary>
        /// Creates a new attack command for entities.
        /// </summary>
        /// <param name="attacker">The attacking entity</param>
        /// <param name="target">The target entity</param>
        public AttackCommand(IEntity attacker, IEntity target)
            : this(attacker?.Id ?? Guid.Empty, target?.Id ?? Guid.Empty)
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

            // Get the attacker
            var attacker = state.GetEntity(_attackerId);
            if (attacker == null)
            {
                return CommandResult.Fail("Attacker not found");
            }

            // Check attacker is alive
            if (!attacker.IsAlive)
            {
                return CommandResult.Fail("Attacker is dead");
            }

            // Check it's the attacker's turn
            if (state.ActiveEntity == null || state.ActiveEntity.Id != _attackerId)
            {
                return CommandResult.Fail("It is not this entity's turn");
            }

            // Get the target
            var target = state.GetEntity(_targetId);
            if (target == null)
            {
                return CommandResult.Fail("Target not found");
            }

            // Check target is alive
            if (!target.IsAlive)
            {
                return CommandResult.Fail("Target is already dead");
            }

            // Check not attacking self
            if (_attackerId == _targetId)
            {
                return CommandResult.Fail("Cannot attack self");
            }

            // Check target is within attack range
            int distance = GridCoordinate.Distance(attacker.Position, target.Position);
            if (distance > attacker.AttackRange)
            {
                return CommandResult.Fail($"Target is out of attack range (distance: {distance}, range: {attacker.AttackRange})");
            }

            return CommandResult.Ok();
        }

        /// <inheritdoc/>
        public void Execute(IGameState state)
        {
            var attacker = state.GetEntity(_attackerId);
            var target = state.GetEntity(_targetId);

            // Apply damage
            state.ApplyDamage(target, attacker.AttackDamage);
        }
    }
}
