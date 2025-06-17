using CorePlatform;
using System; // For StringSplitOptions, Exception, double.TryParse, Guid
using System.IO; // For Path
using System.Reflection; // For Assembly
using System.Windows.Forms; // For UserControl

namespace SamplePlugin
{
    public class MyPlugin : CorePlatform.IScriptablePluginFactory
    {
        // Existing properties and methods that will eventually move to an Instance class
        public string Name => "Sample Test Plugin"; // This is effectively TypeName for the factory
        private string _description = "Description could not be loaded from file."; // Used by TypeDescription
        public string Description => _description; // Kept for now, used by TypeDescription
        private Action<string>? _hostLogCallback; // This might be passed to instances or used by factory


        // --- IPluginFactory Implementation ---
        public string TypeName => Name; // Use existing Name property as TypeName
        public string TypeDescription => Description; // Use existing Description logic for TypeDescription
        public Guid FactoryId { get; } = Guid.NewGuid();

        public void LoadPluginFactory()
        {
            // This replaces the old Load() method's purpose at the factory level.
            // The original Load() logic for reading description is now part of this factory's load.
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string pluginDirectory = Path.GetDirectoryName(assemblyLocation);
            // Use TypeName to construct the description file name, consistent with how it's done in MyPlugin.cs Load()
            string descriptionFileName = this.TypeName + ".md";
            string descriptionFilePath = Path.Combine(pluginDirectory, descriptionFileName);

            Console.WriteLine($"PluginFactory '{TypeName}': Attempting to load description from: {descriptionFilePath}");
            if (File.Exists(descriptionFilePath))
            {
                try
                {
                    _description = File.ReadAllText(descriptionFilePath); // Store in the instance field
                    Console.WriteLine($"PluginFactory '{TypeName}': TypeDescription successfully loaded from {descriptionFilePath}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PluginFactory '{TypeName}': Error reading description file {descriptionFilePath}. Error: {ex.Message}");
                    _description = "Failed to read TypeDescription file. Details: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine($"PluginFactory '{TypeName}': TypeDescription file not found at {descriptionFilePath}. Using fallback description.");
                // Keep existing fallback or set a new one specific to TypeDescription
                // _description = $"Description file ({descriptionFileName}) not found in plugin directory.";
            }
            Console.WriteLine($"PluginFactory '{TypeName}': Loaded successfully.");
        }

        public void UnloadPluginFactory()
        {
            Console.WriteLine($"PluginFactory '{TypeName}': Unloaded.");
            // Original Unload() content:
            // _hostLogCallback?.Invoke($"Plugin '{Name}': Unloaded."); // Instance specific
            // Console.WriteLine($"Plugin '{Name}': Unloaded."); // Instance specific
        }

        public CorePlatform.IPluginInstance CreateInstance(string instanceId, object? initialConfig)
        {
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': CreateInstance called for ID {instanceId}. Actual instance creation not yet implemented.");
            // For now, throwing is safer to indicate it's not ready.
            throw new NotImplementedException($"SamplePlugin instance creation for ID '{instanceId}' is not yet implemented.");
        }

        public System.Windows.Forms.UserControl GetInstanceSettingsUI(object? currentConfig)
        {
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': GetInstanceSettingsUI called. Actual UI not yet implemented.");
            var placeholderControl = new System.Windows.Forms.UserControl();
            var label = new System.Windows.Forms.Label
            {
                Text = $"Settings for {TypeName} Instance - Not Implemented Yet",
                AutoSize = true,
                Padding = new Padding(10)
            };
            placeholderControl.Controls.Add(label);
            placeholderControl.MinimumSize = new System.Drawing.Size(200,100);
            return placeholderControl;
        }
        // --- End IPluginFactory Implementation ---


        // These methods are instance-specific and will be moved to SamplePluginInstance in Stage 1c
        // For now, they are kept here to avoid breaking other parts if they are called (though they shouldn't be directly)
        public void Load() // This is the old Load, effectively instance load
        {
             Console.WriteLine($"Instance of '{TypeName}': Old Load() method called. This should be Initialize() on instance.");
        }

        public void RunTest(Action<string> logCallback)
        {
            // This is instance-specific logic
            var instanceSpecificLogCallback = logCallback ?? _hostLogCallback; // Prefer specific, fallback to factory's
            instanceSpecificLogCallback?.Invoke($"Instance of '{TypeName}': Running test...");
            System.Threading.Thread.Sleep(500);
            instanceSpecificLogCallback?.Invoke($"Instance of '{TypeName}': Step 1 completed.");
            System.Threading.Thread.Sleep(1000);
            instanceSpecificLogCallback?.Invoke($"Instance of '{TypeName}': Step 2 completed.");
            instanceSpecificLogCallback?.Invoke($"Instance of '{TypeName}': Test finished successfully.");
        }

        public void Unload() // This is the old Unload, effectively instance stop/dispose
        {
            _hostLogCallback?.Invoke($"Instance of '{TypeName}': Old Unload() method called. This should be Stop() or Dispose() on instance.");
            Console.WriteLine($"Instance of '{TypeName}': Old Unload() called.");
        }

        // These are script commands for an instance.
        public string? ExecuteScriptCommand(string commandName, string parameters)
        {
            // This logic will move to SamplePluginInstance.
            // For now, if called on factory, it's an error or needs routing if we had a default/single instance.
            // Pretend it's for a conceptual default instance for now.
            Console.WriteLine($"Factory '{TypeName}': ExecuteScriptCommand invoked (should be on an instance). Cmd: {commandName}");
             _hostLogCallback?.Invoke($"Warning: ExecuteScriptCommand called on factory '{TypeName}'. This should target an instance.");


            switch (commandName.ToLowerInvariant())
            {
                case "getstatus":
                    return $"Status from {TypeName} (Factory - should be instance): OK";
                case "echo":
                    return $"Echo from {TypeName} (Factory - should be instance): {parameters}";
                 case "add":
                    try
                    {
                        string[] parts = parameters.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && double.TryParse(parts[0].Trim(), out double a) && double.TryParse(parts[1].Trim(), out double b))
                        {
                            return $"Result of {a} + {b} = {(a + b)} (via {TypeName} Factory)";
                        }
                        return $"Error from {TypeName} Factory: Add command expects two numeric parameters.";
                    }
                    catch (Exception ex)
                    {
                        return $"Error in Add command ({TypeName} Factory): {ex.Message}";
                    }
                default:
                    return $"Error from {TypeName} Factory: Unknown command '{commandName}'.";
            }
        }

        public string[] GetAvailableScriptCommands()
        {
            // These are instance commands. Factory might report "generic" commands or none.
            _hostLogCallback?.Invoke($"Warning: GetAvailableScriptCommands called on factory '{TypeName}'. This should target an instance.");
            return new string[] { "GetStatus (factory)", "Echo (factory)", "Add (factory)" };
        }
    }
}
