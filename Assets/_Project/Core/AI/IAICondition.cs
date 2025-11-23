using SubGame.Core.Commands;
using SubGame.Core.Entities;

namespace SubGame.Core.AI
{
    /// <summary>
    /// Interface for AI conditions that evaluate game state.
    /// Conditions are used in behavior cards to determine which actions to execute.
    /// </summary>
    public interface IAICondition
    {
        /// <summary>
        /// Gets a human-readable description of this condition.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Evaluates the condition against the current game state.
        /// </summary>
        /// <param name="state">The current game state</param>
        /// <param name="monster">The monster evaluating this condition</param>
        /// <param name="context">Shared context for passing data between conditions and actions</param>
        /// <returns>True if the condition is met</returns>
        bool Evaluate(IGameState state, IEntity monster, AIContext context);
    }
}
