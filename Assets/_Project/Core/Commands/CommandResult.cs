namespace SubGame.Core.Commands
{
    /// <summary>
    /// Represents the result of a command validation or execution.
    /// </summary>
    public readonly struct CommandResult
    {
        /// <summary>
        /// Whether the command validation/execution succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Error message if the command failed (null if successful).
        /// </summary>
        public string ErrorMessage { get; }

        private CommandResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static CommandResult Ok() => new CommandResult(true, null);

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        /// <param name="errorMessage">Description of why the command failed</param>
        public static CommandResult Fail(string errorMessage) => new CommandResult(false, errorMessage);

        /// <summary>
        /// Implicit conversion to bool for easy checking.
        /// </summary>
        public static implicit operator bool(CommandResult result) => result.Success;

        /// <summary>
        /// Returns string representation of the result.
        /// </summary>
        public override string ToString()
        {
            return Success ? "Success" : $"Failed: {ErrorMessage}";
        }
    }
}
