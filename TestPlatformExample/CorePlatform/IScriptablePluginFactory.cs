namespace CorePlatform
{
    /// <summary>
    /// Represents a factory for plugins that also support scripting.
    /// Scriptable instances created by this factory will implement IScriptablePluginInstance.
    /// </summary>
    public interface IScriptablePluginFactory : IPluginFactory
    {
        // No additional members are defined for IScriptablePluginFactory at this stage.
        // It serves as a marker interface and ensures that CreateInstance
        // is expected to return an IScriptablePluginInstance.
        //
        // Example of how it would be used by PluginManager (conceptual):
        // if (factory is IScriptablePluginFactory scriptableFactory)
        // {
        //     IScriptablePluginInstance scriptableInstance = (IScriptablePluginInstance)scriptableFactory.CreateInstance(id, config);
        //     // ... use scriptableInstance
        // }
    }
}
