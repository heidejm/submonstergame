namespace SubGame.Core.Commands
{
    /// <summary>
    /// Interface for all game commands.
    /// Commands encapsulate actions that can be validated before execution.
    /// This pattern enables undo/redo, networking, and replay functionality.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Validates whether this command can be executed in the current game state.
        /// </summary>
        /// <param name="state">The current game state</param>
        /// <returns>Result indicating success or failure with error message</returns>
        CommandResult Validate(IGameState state);

        /// <summary>
        /// Executes the command, modifying the game state.
        /// Should only be called after Validate returns success.
        /// </summary>
        /// <param name="state">The game state to modify</param>
        void Execute(IGameState state);
    }
}
