using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI.Actions
{
    /// <summary>
    /// Action that does nothing. Used as a fallback when no other action is possible.
    /// </summary>
    public class IdleAction : IAIAction
    {
        public string Description => "Idle";

        public void Execute(IGameState state, IEntity monster, AIContext context)
        {
            // Do nothing - monster passes its turn
        }
    }
}
