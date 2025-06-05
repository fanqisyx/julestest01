using CorePlatform;

namespace SamplePlugin
{
    public class MyPlugin : IPlugin
    {
        public string Name => "Sample Test Plugin";
        public string Description => "A simple plugin that performs a mock test.";
        private Action<string>? _hostLogCallback;


        public void Load()
        {
            // In a real plugin, initialize resources here
            Console.WriteLine($"{Name} loaded."); // Placeholder
        }

        public void RunTest(Action<string> logCallback)
        {
            _hostLogCallback = logCallback;
            _hostLogCallback($"Plugin '{Name}': Starting test...");
            // Simulate some test steps
            System.Threading.Thread.Sleep(500); // Simulate work
            _hostLogCallback($"Plugin '{Name}': Step 1 completed.");
            System.Threading.Thread.Sleep(1000); // Simulate more work
            _hostLogCallback($"Plugin '{Name}': Step 2 completed.");
            _hostLogCallback($"Plugin '{Name}': Test finished successfully.");
        }

        public void Unload()
        {
            // In a real plugin, release resources here
            _hostLogCallback?.Invoke($"{Name} unloaded.");
            Console.WriteLine($"{Name} unloaded."); // Placeholder
        }
    }
}
