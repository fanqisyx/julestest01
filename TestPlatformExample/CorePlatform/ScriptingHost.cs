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

        // Updated to list available factory TypeNames, as "plugins" are now more nuanced (factories vs instances)
        public string[] ListAvailablePluginTypes() // Renamed for clarity
        {
            return _pluginManager.GetPluginFactories().Select(f => f.TypeName).ToArray();
        }

        // `pluginInstanceId` refers to the InstanceId of an active plugin instance
        public string? ExecuteCommandOnInstance(string pluginInstanceId, string commandName, string parameters) // Renamed for clarity
        {
            if (string.IsNullOrEmpty(pluginInstanceId))
            {
                Log("Script Error: Plugin Instance ID cannot be null or empty for ExecuteCommandOnInstance.");
                return "Error: Plugin Instance ID cannot be null or empty.";
            }

            IPluginInstance? instance = _pluginManager.GetInstanceById(pluginInstanceId);

            if (instance == null)
            {
                Log($"Script Error: Plugin instance '{pluginInstanceId}' not found.");
                return $"Error: Plugin instance '{pluginInstanceId}' not found.";
            }

            if (instance is IScriptablePluginInstance scriptableInstance)
            {
                try
                {
                    Log($"Script: Executing command '{commandName}' on instance '{pluginInstanceId}' (Type: {instance.ParentFactory.TypeName}) with params: '{parameters}'");
                    string? result = scriptableInstance.ExecuteScriptCommand(commandName, parameters);
                    Log($"Script: Command '{commandName}' on instance '{pluginInstanceId}' executed. Result: {(result ?? "null")}");
                    return result;
                }
                catch (Exception ex)
                {
                    Log($"Script Error: Exception executing command '{commandName}' on instance '{pluginInstanceId}': {ex.InnerException?.Message ?? ex.Message}");
                    return $"Error: Exception on instance '{pluginInstanceId}': {ex.InnerException?.Message ?? ex.Message}";
                }
            }
            else
            {
                Log($"Script Error: Plugin instance '{pluginInstanceId}' (Type: {instance.ParentFactory.TypeName}) does not support script commands (does not implement IScriptablePluginInstance).");
                return $"Error: Plugin instance '{pluginInstanceId}' is not scriptable.";
            }
        }

        public void Log(string message)
        {
            // Prefix to distinguish script logs from other system logs if necessary
            _logCallback?.Invoke($"Script> {message}");
        }

        public void print(object? message) // Changed to object? for flexibility
        {
            this.Log(message?.ToString() ?? string.Empty);
        }
    }
}
