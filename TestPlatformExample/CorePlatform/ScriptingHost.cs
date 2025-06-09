using System;
using System.Linq;

namespace CorePlatform
{
    public class ScriptingHost
    {
        private PluginManager _pluginManager;
        private Action<string> _logCallback;

        public ScriptingHost(PluginManager pluginManager, Action<string> logCallback)
        {
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            _logCallback = logCallback ?? throw new ArgumentNullException(nameof(logCallback));
        }

        public string[] ListPluginNames()
        {
            return _pluginManager.GetPlugins().Select(p => p.Name).ToArray();
        }

        public string? ExecutePluginCommand(string pluginName, string commandName, string parameters)
        {
            if (string.IsNullOrEmpty(pluginName))
            {
                Log("Script Error: Plugin name cannot be null or empty for ExecutePluginCommand.");
                return "Error: Plugin name cannot be null or empty.";
            }

            IPlugin? plugin = _pluginManager.GetPlugins().FirstOrDefault(p => p.Name.Equals(pluginName, StringComparison.OrdinalIgnoreCase));

            if (plugin == null)
            {
                Log($"Script Error: Plugin '{pluginName}' not found.");
                return $"Error: Plugin '{pluginName}' not found.";
            }

            if (plugin is IScriptablePlugin scriptablePlugin)
            {
                try
                {
                    Log($"Script: Executing command '{commandName}' on plugin '{pluginName}' with params: '{parameters}'");
                    string? result = scriptablePlugin.ExecuteScriptCommand(commandName, parameters);
                    Log($"Script: Command '{commandName}' on plugin '{pluginName}' executed. Result: {(result ?? "null")}");
                    return result;
                }
                catch (Exception ex)
                {
                    Log($"Script Error: Exception executing command '{commandName}' on plugin '{pluginName}': {ex.InnerException?.Message ?? ex.Message}");
                    return $"Error: Exception on plugin '{pluginName}': {ex.InnerException?.Message ?? ex.Message}";
                }
            }
            else
            {
                Log($"Script Error: Plugin '{pluginName}' does not support script commands (does not implement IScriptablePlugin).");
                return $"Error: Plugin '{pluginName}' is not scriptable.";
            }
        }

        public void Log(string message)
        {
            // Prefix to distinguish script logs from other system logs if necessary
            _logCallback?.Invoke($"Script> {message}");
        }
    }
}
