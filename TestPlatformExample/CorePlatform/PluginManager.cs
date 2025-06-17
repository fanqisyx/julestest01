using System.Reflection;
using System.IO; // Added for Directory and Path operations
using System.Collections.Generic; // For List
using System; // For AppDomain, Guid
using System.Linq; // For FirstOrDefault

namespace CorePlatform
{
    public class PluginManager
    {
        private readonly List<IPluginFactory> _pluginFactories = new List<IPluginFactory>();
        private readonly List<IPluginInstance> _activeInstances = new List<IPluginInstance>();
        private Action<string> _hostLogCallback;
        private readonly string _instanceConfigPath;

        public PluginManager(Action<string> hostLogCallback)
        {
            _hostLogCallback = hostLogCallback;
            // Define a subdirectory for plugin configurations for better organization
            string configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginData");
            if (!Directory.Exists(configDirectory))
            {
                try
                {
                    Directory.CreateDirectory(configDirectory);
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"PluginManager Error: Could not create directory {configDirectory}. Error: {ex.Message}. Instance configurations may fail to save/load.");
                    // Fallback to base directory if sub-directory creation fails
                    configDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }
            }
            _instanceConfigPath = Path.Combine(configDirectory, "plugin_instances.json");
            _hostLogCallback?.Invoke($"PluginManager: Instance configuration path set to: {_instanceConfigPath}");
        }

        public void DiscoverPlugins(string pluginFolderPath)
        {
            _hostLogCallback?.Invoke($"PluginManager: Discovering plugin factories in folder: {pluginFolderPath}");

            if (!Directory.Exists(pluginFolderPath))
            {
                _hostLogCallback?.Invoke($"PluginManager Error: Plugin folder '{pluginFolderPath}' not found.");
                return;
            }

            _pluginFactories.Clear();
            _activeInstances.Clear(); // Clearing factories means active instances from those factories are no longer valid
            _hostLogCallback?.Invoke("PluginManager: Cleared existing plugin factories and active instances before discovery.");

            string[] dllFiles = Directory.GetFiles(pluginFolderPath, "*.dll");

            if (dllFiles.Length == 0)
            {
                _hostLogCallback?.Invoke($"PluginManager: No DLLs found in plugin folder: {pluginFolderPath}");
                return;
            }

            foreach (string dllPath in dllFiles)
            {
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(dllPath);
                    var factoryTypes = pluginAssembly.GetTypes()
                        .Where(t => typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in factoryTypes)
                    {
                        try
                        {
                            IPluginFactory? factory = Activator.CreateInstance(type) as IPluginFactory;
                            if (factory != null)
                            {
                                // Check for duplicates by FactoryId or TypeName before adding
                                if (!_pluginFactories.Any(f => f.FactoryId == factory.FactoryId || f.TypeName == factory.TypeName))
                                {
                                    factory.LoadPluginFactory(); // Call LoadPluginFactory after instantiation
                                    _pluginFactories.Add(factory);
                                    _hostLogCallback?.Invoke($"PluginManager: Successfully loaded plugin factory: {factory.TypeName} (ID: {factory.FactoryId}) from {Path.GetFileName(dllPath)}");
                                }
                                else
                                {
                                    _hostLogCallback?.Invoke($"PluginManager: Plugin factory '{factory.TypeName}' from {Path.GetFileName(dllPath)} already loaded or ID conflict. Skipping.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _hostLogCallback?.Invoke($"PluginManager Error: Error instantiating plugin factory type '{type.FullName}' from {Path.GetFileName(dllPath)}: {ex.Message}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    _hostLogCallback?.Invoke($"PluginManager Error: Error loading types from assembly {Path.GetFileName(dllPath)}: {ex.Message}");
                    foreach (var loaderEx in ex.LoaderExceptions ?? Enumerable.Empty<Exception?>())
                    {
                        if (loaderEx != null) _hostLogCallback?.Invoke($"  LoaderException: {loaderEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"PluginManager Error: Error loading assembly {Path.GetFileName(dllPath)}: {ex.Message}");
                }
            }

            if (!_pluginFactories.Any())
            {
                _hostLogCallback?.Invoke("PluginManager: No plugin factories were successfully loaded.");
            }
        }

        public IEnumerable<IPluginFactory> GetPluginFactories()
        {
            return _pluginFactories.AsReadOnly();
        }

        public IEnumerable<IPluginInstance> GetAllInstances()
        {
            return _activeInstances.AsReadOnly();
        }

        public IPluginInstance? GetInstanceById(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return null;
            return _activeInstances.FirstOrDefault(inst => inst.InstanceId.Equals(instanceId, StringComparison.OrdinalIgnoreCase));
        }

        public void RunPluginTests(Action<string> logCallback)
        {
            _hostLogCallback?.Invoke("PluginManager: Starting tests on all active instances...");
            if (!_activeInstances.Any())
            {
                _hostLogCallback?.Invoke("PluginManager: No active instances to test.");
                return;
            }

            foreach (var instance in _activeInstances)
            {
                try
                {
                    _hostLogCallback?.Invoke($"PluginManager: Running test for instance: {instance.InstanceId} (Type: {instance.ParentFactory.TypeName})...");
                    instance.RunTest(logCallback); // Pass the specific logCallback for this test run
                    _hostLogCallback?.Invoke($"PluginManager: Test completed for instance: {instance.InstanceId}.");
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"PluginManager: Error running test for instance {instance.InstanceId}. Error: {ex.Message}");
                    // Optionally, log the full exception details if needed
                }
            }
            _hostLogCallback?.Invoke("PluginManager: Finished running tests on all instances.");
        }
    }
}
