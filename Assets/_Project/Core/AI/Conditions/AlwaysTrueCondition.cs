using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI.Conditions
{
    /// <summary>
    /// Condition that always returns true.
    /// Useful for fallback branches or unconditional actions.
    /// </summary>
    public class AlwaysTrueCondition : IAICondition
    {
        public string Description => "Always";

        public bool Evaluate(IGameState state, IEntity monster, AIContext context)
        {
            return true;
        }
    }
}
