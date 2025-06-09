using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CorePlatform
{
    public class ScriptEngine
    {
        private ScriptOptions? _scriptOptions; // Made nullable
        private Action<string> _hostLogCallback; // For engine's own logging

        public ScriptEngine(Action<string> hostLogCallback)
        {
            _hostLogCallback = hostLogCallback;
            InitializeScriptOptions();
        }

        private void InitializeScriptOptions()
        {
            _scriptOptions = ScriptOptions.Default
                .AddReferences(
                    Assembly.GetAssembly(typeof(object)), // System.Runtime / mscorlib / System.Private.CoreLib
                    Assembly.GetAssembly(typeof(System.Linq.Enumerable)), // System.Linq
                    Assembly.GetAssembly(typeof(IPlugin)), // CorePlatform.dll (for IPlugin, IScriptablePlugin)
                    Assembly.GetAssembly(typeof(ScriptingHost)) // CorePlatform.dll (for ScriptingHost)
                                                                // Potentially add more common assemblies scripts might need
                )
                .AddImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "CorePlatform" // So scripts can use IPlugin, ScriptingHost without full namespace
                );
        }

        public async Task<ScriptExecutionResult> ExecuteScriptAsync(string scriptText, PluginManager pluginManager, Action<string> uiLogCallbackForScriptHost)
        {
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                return new ScriptExecutionResult { Success = false, ErrorMessage = "Script text cannot be empty." };
            }

            var scriptHost = new ScriptingHost(pluginManager, uiLogCallbackForScriptHost);

            try
            {
                _hostLogCallback?.Invoke("ScriptEngine: Executing script...");
                var scriptState = await CSharpScript.RunAsync(scriptText, _scriptOptions, globals: scriptHost, globalsType: typeof(ScriptingHost));
                _hostLogCallback?.Invoke("ScriptEngine: Script execution completed.");

                if (scriptState.ReturnValue != null)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Script returned value: {scriptState.ReturnValue}");
                    return new ScriptExecutionResult { Success = true, ReturnValue = scriptState.ReturnValue };
                }
                return new ScriptExecutionResult { Success = true };
            }
            catch (CompilationErrorException cex)
            {
                _hostLogCallback?.Invoke($"ScriptEngine: Compilation error: {cex.Message}");
                var diagnostics = string.Join(Environment.NewLine, cex.Diagnostics.Select(d => d.ToString()));
                _hostLogCallback?.Invoke($"Diagnostics:\n{diagnostics}");
                return new ScriptExecutionResult { Success = false, ErrorMessage = "Script compilation failed.", CompilationErrors = cex.Diagnostics.Select(d => d.ToString()).ToList() };
            }
            catch (Exception ex)
            {
                _hostLogCallback?.Invoke($"ScriptEngine: Runtime error: {ex.Message}");
                return new ScriptExecutionResult { Success = false, ErrorMessage = $"Script runtime error: {ex.Message}" };
            }
        }
    }

    public class ScriptExecutionResult
    {
        public bool Success { get; set; }
        public object? ReturnValue { get; set; }
        public string? ErrorMessage { get; set; }
        public System.Collections.Generic.List<string>? CompilationErrors { get; set; }
    }
}
