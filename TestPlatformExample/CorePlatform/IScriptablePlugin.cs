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
    }
}
