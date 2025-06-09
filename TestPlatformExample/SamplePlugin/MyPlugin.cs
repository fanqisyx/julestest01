using CorePlatform;
using System; // For StringSplitOptions, Exception, double.TryParse

namespace SamplePlugin
{
    public class MyPlugin : IScriptablePlugin // Implements IScriptablePlugin
    {
        public string Name => "Sample Test Plugin";
        public string Description => "A simple plugin that performs a mock test and supports script commands.";
        private Action<string>? _hostLogCallback;


        public void Load()
        {
            // In a real plugin, initialize resources here
            Console.WriteLine($"Plugin '{Name}': Loaded.");
        }

        public void RunTest(Action<string> logCallback)
        {
            _hostLogCallback = logCallback;
            _hostLogCallback?.Invoke($"Plugin '{Name}': Starting test...");
            // Simulate some test steps
            System.Threading.Thread.Sleep(500); // Simulate work
            _hostLogCallback?.Invoke($"Plugin '{Name}': Step 1 completed.");
            System.Threading.Thread.Sleep(1000); // Simulate more work
            _hostLogCallback?.Invoke($"Plugin '{Name}': Step 2 completed.");
            _hostLogCallback?.Invoke($"Plugin '{Name}': Test finished successfully.");
        }

        public void Unload()
        {
            // In a real plugin, release resources here
            _hostLogCallback?.Invoke($"Plugin '{Name}': Unloaded.");
            Console.WriteLine($"Plugin '{Name}': Unloaded.");
        }

        // Implementation of IScriptablePlugin.ExecuteScriptCommand
        public string? ExecuteScriptCommand(string commandName, string parameters)
        {
            // Log entry of this command via console for plugin's own diagnostics if needed
            Console.WriteLine($"Plugin '{Name}': Script command '{commandName}' received. Params: '{parameters}'");

            switch (commandName.ToLowerInvariant())
            {
                case "getstatus":
                    return "Status: OK, All systems nominal.";
                case "echo":
                    return $"Echo from {Name}: {parameters}";
                case "add":
                    try
                    {
                        string[] parts = parameters.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && double.TryParse(parts[0].Trim(), out double a) && double.TryParse(parts[1].Trim(), out double b))
                        {
                            return $"Result of {a} + {b} = {(a + b)}";
                        }
                        return "Error: Add command expects two numeric parameters separated by comma or semicolon (e.g., '1,2').";
                    }
                    catch (Exception ex)
                    {
                        return $"Error processing Add command in {Name}: {ex.Message}";
                    }
                default:
                    return $"Error: Unknown command '{commandName}' for plugin '{Name}'.";
            }
        }
    }
}
