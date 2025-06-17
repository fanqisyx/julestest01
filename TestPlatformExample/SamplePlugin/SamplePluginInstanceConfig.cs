using System;

namespace SamplePlugin
{
    /// <summary>
    /// Holds configuration specific to an instance of SamplePlugin.
    /// </summary>
    public class SamplePluginInstanceConfig
    {
        /// <summary>
        /// Gets or sets the display name for this specific instance.
        /// This name can be shown in logs or UI elements related to this instance.
        /// </summary>
        public string? InstanceDisplayName { get; set; }

        // Add other instance-specific properties here as needed in the future.
        // For example:
        // public int TargetPort { get; set; }
        // public string ApiKey { get; set; }

        public SamplePluginInstanceConfig()
        {
            // Initialize with default values if necessary
            InstanceDisplayName = "Unnamed SamplePlugin Instance";
        }
    }
}
