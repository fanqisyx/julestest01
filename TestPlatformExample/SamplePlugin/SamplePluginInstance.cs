using System;
using CorePlatform; // For IPluginInstance, IScriptablePluginInstance, IPluginFactory
using System.Linq; // For Enumerable.Empty
using System.Windows.Forms; // For MessageBox if any command uses it (though typically Host.ShowMessageBox is better)
using System.Collections.Generic; // For List, Dictionary

namespace SamplePlugin
{
    public class SamplePluginInstance : IScriptablePluginInstance
    {
        private SamplePluginInstanceConfig _config;
        private Action<string>? _hostLogCallback;

        public string InstanceId { get; }
        public IPluginFactory ParentFactory { get; }
        public string InstanceDisplayName { get; private set; }

        public SamplePluginInstance(
            SamplePluginFactory factory,
            string instanceId,
            SamplePluginInstanceConfig? config,
            Action<string>? hostLogCallback)
        {
            ParentFactory = factory;
            InstanceId = instanceId;
            _config = config ?? new SamplePluginInstanceConfig(); // Ensure config is not null
            InstanceDisplayName = _config.InstanceDisplayName ?? instanceId; // Fallback for display name
            _hostLogCallback = hostLogCallback;

            // Initialize will be called by PluginManager after construction
        }

        public void Initialize(object? config)
        {
            _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Initializing...");
            if (config is SamplePluginInstanceConfig instanceConfig)
            {
                _config = instanceConfig;
                InstanceDisplayName = _config.InstanceDisplayName ?? InstanceId; // Update display name
                _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Configuration applied. Display Name: '{InstanceDisplayName}'.");
            }
            else if (config == null)
            {
                 _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Initialized with no specific configuration (or default). Display Name: '{InstanceDisplayName}'.");
            }
            else
            {
                _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Warning - Initialize received unexpected config type: {config.GetType().FullName}. Using previous/default config.");
            }
        }

        public void Start()
        {
            _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Started.");
            // Perform any startup actions for the instance here
        }

        public void Stop()
        {
            _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Stopped.");
            // Perform any cleanup or shutdown actions for the instance here
        }

        public object? GetCurrentConfig()
        {
            return _config;
        }

        public void RunTest(Action<string> logCallback)
        {
            // This is the old RunTest logic from MyPlugin/SamplePluginFactory
            logCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Starting test...");
            System.Threading.Thread.Sleep(500);
            logCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Step 1 completed.");
            System.Threading.Thread.Sleep(1000);
            logCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Step 2 completed.");
            logCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Test finished successfully.");
        }

        // --- IScriptablePluginInstance Methods ---

        public string? ExecuteScriptCommand(string commandName, string parameters)
        {
            // This is the old ExecuteScriptCommand logic from MyPlugin/SamplePluginFactory
            _hostLogCallback?.Invoke($"Instance '{InstanceId} ({InstanceDisplayName})': Script command '{commandName}' received. Params: '{parameters}'");

            switch (commandName.ToLowerInvariant())
            {
                case "getstatus":
                    return $"Status for '{InstanceDisplayName} ({InstanceId})': OK, All systems nominal.";
                case "getinstancename": // New command
                    return InstanceDisplayName;
                case "echo":
                    return $"Echo from '{InstanceDisplayName} ({InstanceId})': {parameters}";
                case "add":
                    try
                    {
                        string[] parts = parameters.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && double.TryParse(parts[0].Trim(), out double a) && double.TryParse(parts[1].Trim(), out double b))
                        {
                            return $"Result of {a} + {b} for instance '{InstanceDisplayName} ({InstanceId})' = {(a + b)}";
                        }
                        return $"Error for '{InstanceDisplayName} ({InstanceId})': Add command expects two numeric parameters separated by comma or semicolon (e.g., '1,2').";
                    }
                    catch (Exception ex)
                    {
                        return $"Error processing Add command in '{InstanceDisplayName} ({InstanceId})': {ex.Message}";
                    }
                default:
                    return $"Error for '{InstanceDisplayName} ({InstanceId})': Unknown command '{commandName}'.";
            }
        }

        public string[] GetAvailableScriptCommands()
        {
            // This is the old GetAvailableScriptCommands logic
            return new string[] { "GetStatus", "GetInstanceName", "Echo", "Add" };
        }
    }
}
