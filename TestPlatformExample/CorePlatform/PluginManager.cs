using System.Reflection;

namespace CorePlatform
{
    public class PluginManager
    {
        private List<IPlugin> _plugins = new List<IPlugin>();
        private Action<string> _logCallback;

        public PluginManager(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        // In a real app, this would scan assemblies in a plugins folder.
        // For this example, we'll manually add known plugins or use reflection on loaded assemblies.
        public void DiscoverPlugins()
        {
            _logCallback("Discovering plugins...");
            // Simplified discovery: Manually add the SamplePlugin for this example.
            // Or, more advanced: iterate through assemblies in the output directory.
            // For now, we will rely on the UI project referencing plugins directly or loading them explicitly.

            // Example of discovering plugins via reflection from currently loaded assemblies:
            var pluginAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic && asm.Location.Contains("SamplePlugin")) // Crude filter
                .ToList();

            foreach (var assembly in pluginAssemblies)
            {
                try
                {
                    var types = assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface);
                    foreach (var type in types)
                    {
                        try
                        {
                            var plugin = Activator.CreateInstance(type) as IPlugin;
                            if (plugin != null)
                            {
                                _plugins.Add(plugin);
                                _logCallback($"Found plugin: {plugin.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logCallback($"Error instantiating plugin '{type.FullName}': {ex.Message}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var loaderMessages = ex.LoaderExceptions?.Select(e => e?.Message) ?? Enumerable.Empty<string>();
                    _logCallback($"Error loading types from assembly '{assembly.FullName}': {string.Join(", ", loaderMessages.Where(m => m != null))}");
                }
                catch (Exception ex)
                {
                    _logCallback($"Error inspecting assembly '{assembly.FullName}': {ex.Message}");
                }
            }
            if (!_plugins.Any())
            {
                _logCallback("No plugins discovered via reflection. Ensure SamplePlugin is referenced and built.");
            }
        }

        public void AddPlugin(IPlugin plugin) // Allow manual addition for simplicity
        {
            if (!_plugins.Contains(plugin))
            {
                _plugins.Add(plugin);
                _logCallback($"Manually added plugin: {plugin.Name}");
                plugin.Load();
            }
        }

        public List<IPlugin> GetPlugins()
        {
            return _plugins;
        }

        public void RunPluginTests(Action<string> logCallback)
        {
            if (!_plugins.Any())
            {
                logCallback("No plugins loaded to run tests.");
                return;
            }
            foreach (var plugin in _plugins)
            {
                logCallback($"--- Running Test for Plugin: {plugin.Name} ---");
                plugin.RunTest(logCallback);
                logCallback($"--- Test Finished for Plugin: {plugin.Name} ---");
            }
        }
    }
}
