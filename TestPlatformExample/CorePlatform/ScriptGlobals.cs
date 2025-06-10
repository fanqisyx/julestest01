using System;

namespace CorePlatform
{
    // This class will be the actual 'globals' object for Roslyn scripts.
    // It exposes the ScriptingHost instance via a public property named 'Host'.
    public class ScriptGlobals
    {
        public ScriptingHost Host { get; }

        public ScriptGlobals(ScriptingHost hostInstance)
        {
            Host = hostInstance ?? throw new ArgumentNullException(nameof(hostInstance));
        }
    }
}
