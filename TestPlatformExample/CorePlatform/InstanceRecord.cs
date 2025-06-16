using System;

namespace CorePlatform
{
    /// <summary>
    /// Represents the data required to persist and recreate a plugin instance.
    /// </summary>
    public class InstanceRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for the plugin instance.
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the factory that created/can create this instance.
        /// </summary>
        public Guid FactoryId { get; set; }

        /// <summary>
        /// Gets or sets the type name of the factory.
        /// Used for display purposes and potentially for locating the factory if FactoryId is missing or changes.
        /// </summary>
        public string FactoryTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plugin-specific configuration data for this instance.
        /// The structure of this object is determined by the plugin itself.
        /// For robust serialization (e.g., with System.Text.Json), this might often be
        /// a Dictionary<string, object> or a JsonElement if types are not known by CorePlatform,
        /// or a concrete type if the plugin shares its configuration DTO.
        /// For now, 'object' allows flexibility.
        /// </summary>
        public object? Configuration { get; set; }

        /// <summary>
        /// Default constructor for serialization purposes.
        /// </summary>
        public InstanceRecord() { }

        /// <summary>
        /// Convenience constructor.
        /// </summary>
        public InstanceRecord(string instanceId, Guid factoryId, string factoryTypeName, object? configuration)
        {
            InstanceId = instanceId;
            FactoryId = factoryId;
            FactoryTypeName = factoryTypeName;
            Configuration = configuration;
        }
    }
}
