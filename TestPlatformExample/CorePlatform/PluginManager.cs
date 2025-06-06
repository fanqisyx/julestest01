using System.Reflection;
using System.IO; // Added for Directory and Path operations

namespace CorePlatform
{
    public class PluginManager
    {
        private List<IPlugin> _plugins = new List<IPlugin>();
        private Action<string> _logCallback;

        public PluginManager(Action<string> logCallback)
        {
            _logCallback = logCallback;
            // Initialize _plugins if it's null, though field initializer does this.
            // _plugins = _plugins ?? new List<IPlugin>();
        }

        public void DiscoverPlugins(string pluginFolderPath)
        {
            _logCallback($"Discovering plugins in folder: {pluginFolderPath}");

            if (!Directory.Exists(pluginFolderPath))
            {
                _logCallback($"Error: Plugin folder '{pluginFolderPath}' not found.");
                return;
            }

            // Optional: Clear existing plugins if this is a refresh operation
            // _plugins.Clear();
            // _logCallback("Cleared existing plugins before discovery.");

            string[] dllFiles = Directory.GetFiles(pluginFolderPath, "*.dll");

            if (dllFiles.Length == 0)
            {
                _logCallback($"No DLLs found in plugin folder: {pluginFolderPath}");
                return;
            }

            foreach (string dllPath in dllFiles)
            {
                try
                {
                    // For more advanced scenarios (e.g., unloading, versioning),
                    // consider using AssemblyLoadContext.
                    Assembly pluginAssembly = Assembly.LoadFrom(dllPath);
                    var pluginTypes = pluginAssembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        try
                        {
                            IPlugin? plugin = Activator.CreateInstance(type) as IPlugin;
                            if (plugin != null)
                            {
                                // Basic duplicate check by Name. More robust checks might involve version or full type name.
                                if (!_plugins.Any(p => p.Name == plugin.Name))
                                {
                                    _plugins.Add(plugin);
                                    plugin.Load(); // Call Load after adding
                                    _logCallback($"Successfully loaded plugin: {plugin.Name} from {Path.GetFileName(dllPath)}");
                                }
                                else
                                {
                                    _logCallback($"Plugin '{plugin.Name}' from {Path.GetFileName(dllPath)} already loaded. Skipping.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logCallback($"Error instantiating plugin type '{type.FullName}' from {Path.GetFileName(dllPath)}: {ex.Message}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex) // Specifically catch this for loader exceptions
                {
                    _logCallback($"Error loading types from assembly {Path.GetFileName(dllPath)}: {ex.Message}");
                    foreach (var loaderEx in ex.LoaderExceptions ?? Enumerable.Empty<Exception?>())
                    {
                        if (loaderEx != null) _logCallback($"  LoaderException: {loaderEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logCallback($"Error loading assembly {Path.GetFileName(dllPath)}: {ex.Message}");
                }
            }

            if (!_plugins.Any())
            {
                _logCallback("No plugins were successfully loaded.");
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
