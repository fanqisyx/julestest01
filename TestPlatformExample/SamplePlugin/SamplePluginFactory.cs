using CorePlatform;
using System; // For StringSplitOptions, Exception, double.TryParse, Guid
using System.IO; // For Path
using System.Reflection; // For Assembly
using System.Windows.Forms; // For UserControl

namespace SamplePlugin
{
    public class SamplePluginFactory : CorePlatform.IScriptablePluginFactory
    {
        private Action<string>? _hostLogCallback;

        // --- IPluginFactory Implementation ---
        public string TypeName => "SamplePlugin";
        public string TypeDescription { get; private set; } = "Description for SamplePlugin type could not be loaded.";
        public Guid FactoryId { get; } = Guid.NewGuid();

        public SamplePluginFactory(Action<string>? hostLogCallback = null)
        {
            _hostLogCallback = hostLogCallback;
        }

        public void LoadPluginFactory()
        {
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': Loading factory...");
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string pluginDirectory = Path.GetDirectoryName(assemblyLocation) ?? "";
            string descriptionFileName = this.TypeName + ".md";
            string descriptionFilePath = Path.Combine(pluginDirectory, descriptionFileName);

            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': Attempting to load TypeDescription from: {descriptionFilePath}");
            if (File.Exists(descriptionFilePath))
            {
                try
                {
                    TypeDescription = File.ReadAllText(descriptionFilePath);
                    _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': TypeDescription successfully loaded.");
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': Error reading TypeDescription file {descriptionFilePath}. Error: {ex.Message}");
                    TypeDescription = $"Failed to read TypeDescription file. Details: {ex.Message}";
                }
            }
            else
            {
                _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': TypeDescription file not found at {descriptionFilePath}. Using fallback.");
                TypeDescription = $"Description file ({descriptionFileName}) not found in plugin directory '{pluginDirectory}'.";
            }
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': Factory loaded.");
        }

        public void UnloadPluginFactory()
        {
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': Unloaded.");
        }

        public CorePlatform.IPluginInstance CreateInstance(string instanceId, object? initialConfig)
        {
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': CreateInstance called for ID '{instanceId}'.");
            // Assuming SamplePluginInstance constructor: SamplePluginInstance(IPluginFactory parentFactory, string instanceId, SamplePluginInstanceConfig config, Action<string> logCallback)
            return new SamplePluginInstance(this, instanceId, initialConfig as SamplePluginInstanceConfig, _hostLogCallback);
        }

        public System.Windows.Forms.UserControl GetInstanceSettingsUI(object? currentConfig)
        {
            _hostLogCallback?.Invoke($"PluginFactory '{TypeName}': GetInstanceSettingsUI called.");
            return new SamplePluginSettingsUI(currentConfig as SamplePluginInstanceConfig);
        }
        // --- End IPluginFactory Implementation ---

        // No instance-specific methods remain in the factory.
        // IScriptablePluginFactory itself adds no members, it's a marker for now.
    }
}
