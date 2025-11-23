using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI
{
    /// <summary>
    /// Interface for AI actions that modify game state.
    /// Actions are executed when their associated conditions are met.
    /// </summary>
    public interface IAIAction
    {
        /// <summary>
        /// Gets a human-readable description of this action.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="state">The current game state</param>
        /// <param name="monster">The monster executing this action</param>
        /// <param name="context">Shared context containing data from condition evaluation</param>
        void Execute(IGameState state, IEntity monster, AIContext context);
    }
}
