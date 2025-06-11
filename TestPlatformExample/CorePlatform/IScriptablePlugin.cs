namespace CorePlatform
{
    public interface IScriptablePlugin : IPlugin
    {
        /// <summary>
        /// Executes a command passed from a script.
        /// </summary>
        /// <param name="commandName">The name of the command to execute.</param>
        /// <param name="parameters">Parameters for the command.</param>
        /// <returns>A result string, or null if no direct string result.</returns>
        string? ExecuteScriptCommand(string commandName, string parameters);

        /// <summary>
        /// Gets an array of available script command names that this plugin supports.
        /// </summary>
        /// <returns>An array of strings, where each string is a command name.</returns>
        string[] GetAvailableScriptCommands(); // New method
    }
}
