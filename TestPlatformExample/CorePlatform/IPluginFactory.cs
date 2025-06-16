using System;
using System.Windows.Forms; // Required for UserControl

namespace CorePlatform
{
    public interface IPluginFactory
    {
        /// <summary>
        /// Gets the unique type name of the plugin (e.g., "SamplePlugin", "RoboticArmPlugin").
        /// This name will be used for identifying the plugin type and potentially for finding its description file.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Gets a human-readable description of the plugin type.
        /// Typically loaded from an external file (e.g., <TypeName>.md).
        /// </summary>
        string TypeDescription { get; }

        /// <summary>
        /// Gets a unique identifier for this loaded factory.
        /// Useful for associating instances with their creator.
        /// </summary>
        Guid FactoryId { get; }

        /// <summary>
        /// Called when the plugin factory is first loaded by the PluginManager.
        /// Use this to load resources common to all instances or to read the TypeDescription.
        /// </summary>
        void LoadPluginFactory();

        /// <summary>
        /// Called when the plugin factory is being unloaded by the PluginManager.
        /// Use this to release any common resources.
        /// </summary>
        void UnloadPluginFactory();

        /// <summary>
        /// Creates a new instance of the plugin.
        /// </summary>
        /// <param name="instanceId">A unique identifier for the new instance.</param>
        /// <param name="initialConfig">Optional initial configuration for the instance.
        /// This configuration object is typically obtained from GetInstanceSettingsUI.</param>
        /// <returns>A new plugin instance.</returns>
        IPluginInstance CreateInstance(string instanceId, object? initialConfig);

        /// <summary>
        /// Gets a UserControl that allows configuration of a plugin instance.
        /// This UI will be hosted by the main application.
        /// </summary>
        /// <param name="currentConfig">The current configuration of an existing instance,
        /// or null if configuring a new instance. The UserControl should populate itself based on this.</param>
        /// <returns>A UserControl for instance settings.</returns>
        UserControl GetInstanceSettingsUI(object? currentConfig);
    }
}
