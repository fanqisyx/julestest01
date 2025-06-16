using System;

namespace CorePlatform
{
    public interface IPluginInstance
    {
        /// <summary>
        /// Gets the unique identifier for this instance (e.g., "RoboticArm1", "SampleInstanceAlpha").
        /// </summary>
        string InstanceId { get; }

        /// <summary>
        /// Gets the factory that created this instance.
        /// </summary>
        IPluginFactory ParentFactory { get; }

        /// <summary>
        /// Initializes the plugin instance with its specific configuration.
        /// This is typically called after CreateInstance by the PluginManager.
        /// </summary>
        /// <param name="config">The configuration object for this instance.
        /// The type and structure of this object are defined by the plugin factory.</param>
        void Initialize(object? config);

        /// <summary>
        /// Starts the plugin instance, allowing it to perform its operations.
        /// (e.g., connect to a device, start background tasks).
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the plugin instance, halting its operations and releasing resources.
        /// (e.g., disconnect from a device, stop background tasks).
        /// </summary>
        void Stop();

        /// <summary>
        /// Runs a specific test routine for this instance.
        /// </summary>
        /// <param name="logCallback">A callback action to log messages during the test.</param>
        void RunTest(Action<string> logCallback);

        /// <summary>
        /// Retrieves the current configuration of this instance.
        /// Used when re-opening the settings UI for an existing instance.
        /// </summary>
        /// <returns>The current configuration object.</returns>
        object? GetCurrentConfig();
    }
}
