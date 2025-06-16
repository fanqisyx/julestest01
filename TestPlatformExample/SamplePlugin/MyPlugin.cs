using CorePlatform;
using System; // For StringSplitOptions, Exception, double.TryParse
using System.IO; // For Path
using System.Reflection; // For Assembly

namespace SamplePlugin
{
    public class MyPlugin : IScriptablePlugin
    {
        public string Name => "Sample Test Plugin";
        private string _description = "Description could not be loaded from file.";
        public string Description => _description;
        private Action<string>? _hostLogCallback;


        public void Load()
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string pluginDirectory = Path.GetDirectoryName(assemblyLocation);
            string descriptionFileName = Path.GetFileNameWithoutExtension(assemblyLocation) + ".md";
            string descriptionFilePath = Path.Combine(pluginDirectory, descriptionFileName);

            Console.WriteLine($"Plugin '{Name}': Attempting to load description from: {descriptionFilePath}");

            if (File.Exists(descriptionFilePath))
            {
                try
                {
                    _description = File.ReadAllText(descriptionFilePath);
                    Console.WriteLine($"Plugin '{Name}': Description successfully loaded from {descriptionFilePath}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Plugin '{Name}': Error reading description file {descriptionFilePath}. Error: {ex.Message}");
                    _description = "Failed to read description file. Details: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine($"Plugin '{Name}': Description file not found at {descriptionFilePath}. Using fallback description.");
                _description = $"Description file ({descriptionFileName}) not found in plugin directory.";
            }

            Console.WriteLine($"Plugin '{Name}': Loaded.");
        }

        public void RunTest(Action<string> logCallback)
        {
            _hostLogCallback = logCallback;
            _hostLogCallback?.Invoke($"Plugin '{Name}': Starting test...");
            System.Threading.Thread.Sleep(500);
            _hostLogCallback?.Invoke($"Plugin '{Name}': Step 1 completed.");
            System.Threading.Thread.Sleep(1000);
            _hostLogCallback?.Invoke($"Plugin '{Name}': Step 2 completed.");
            _hostLogCallback?.Invoke($"Plugin '{Name}': Test finished successfully.");
        }

        public void Unload()
        {
            _hostLogCallback?.Invoke($"Plugin '{Name}': Unloaded.");
            Console.WriteLine($"Plugin '{Name}': Unloaded.");
        }

        public string? ExecuteScriptCommand(string commandName, string parameters)
        {
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

        public string[] GetAvailableScriptCommands()
        {
            return new string[] { "GetStatus", "Echo", "Add" };
        }
    }
}
