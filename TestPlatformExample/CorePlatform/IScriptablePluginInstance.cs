namespace CorePlatform
{
    /// <summary>
    /// Represents a plugin instance that supports execution of script commands.
    /// </summary>
    public interface IScriptablePluginInstance : IPluginInstance
    {
        /// <summary>
        /// Executes a command specific to this plugin instance.
        /// </summary>
        /// <param name="commandName">The name of the command to execute.</param>
        /// <param name="parameters">Parameters for the command.</param>
        /// <returns>A string result or null if the command produces no direct string output.</returns>
        string? ExecuteScriptCommand(string commandName, string parameters);

        /// <summary>
        /// Gets an array of available script command names that this instance supports.
        /// </summary>
        /// <returns>An array of command names.</returns>
        string[] GetAvailableScriptCommands();
    }
}
