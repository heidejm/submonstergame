using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI.Actions
{
    /// <summary>
    /// Action that moves the monster toward its target.
    /// Uses the target position set in context by a condition.
    /// </summary>
    public class MoveTowardTargetAction : IAIAction
    {
        public string Description => "Move toward target";

        public void Execute(IGameState state, IEntity monster, AIContext context)
        {
            if (!context.TargetPosition.HasValue)
                return;

            // Move the monster to the calculated position
            state.MoveEntity(monster, context.TargetPosition.Value);
        }
    }
}
