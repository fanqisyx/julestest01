using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis; // Added for DiagnosticSeverity
using System.Collections.Generic; // Added for List<string>

namespace CorePlatform
{
    public class ScriptEngine
    {
        private ScriptOptions? _scriptOptions;
        private Action<string> _hostLogCallback;

        public ScriptEngine(Action<string> hostLogCallback)
        {
            _hostLogCallback = hostLogCallback;
            InitializeScriptOptions();
        }

        private void InitializeScriptOptions()
        {
            _scriptOptions = ScriptOptions.Default
                .AddReferences(
                    Assembly.GetAssembly(typeof(object)),
                    Assembly.GetAssembly(typeof(System.Linq.Enumerable)),
                    Assembly.GetAssembly(typeof(IPlugin)),
                    Assembly.GetAssembly(typeof(ScriptingHost))
                )
                .AddImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "CorePlatform"
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

        public ScriptCompilationCheckResult CheckSyntax(string scriptText) // Changed to synchronous
        {
            var result = new ScriptCompilationCheckResult();
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                // An empty script has no syntax errors.
                result.Success = true;
                return result;
            }

            try
            {
                _hostLogCallback?.Invoke("ScriptEngine: Checking script syntax...");
                if (_scriptOptions == null)
                {
                    _hostLogCallback?.Invoke("ScriptEngine Error: ScriptOptions not initialized!");
                    result.Diagnostics.Add("Engine Error: ScriptOptions not initialized.");
                    result.Success = false;
                    return result;
                }

                var script = CSharpScript.Create(scriptText, _scriptOptions, globalsType: typeof(ScriptingHost));
                var compilation = script.GetCompilation();
                var diagnostics = compilation.GetDiagnostics(); // This is an ImmutableArray<Diagnostic>

                result.Success = !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in diagnostics)
                {
                    result.Diagnostics.Add(diagnostic.ToString());
                }

                if (result.Success)
                {
                    _hostLogCallback?.Invoke("ScriptEngine: Syntax check completed. No errors found.");
                }
                else
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Syntax check completed. Errors found: {result.Diagnostics.Count(d => d.ToLowerInvariant().Contains("error"))}");
                }
            }
            catch (Exception ex)
            {
                _hostLogCallback?.Invoke($"ScriptEngine: Error during syntax check: {ex.Message}");
                result.Diagnostics.Add($"Unexpected error during syntax check: {ex.Message}");
                result.Success = false;
            }
            return result;
        }
    }

    public class ScriptExecutionResult
    {
        public bool Success { get; set; }
        public object? ReturnValue { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? CompilationErrors { get; set; }
    }

    public class ScriptCompilationCheckResult
    {
        public bool Success { get; set; } // True if no errors (warnings might still exist)
        public List<string> Diagnostics { get; set; } = new List<string>();
        // Helper property to explicitly check if any diagnostic string contains "error"
        // This can be more robust if Diagnostic objects were stored, but string list is used here.
        public bool HasErrors => Diagnostics.Any(d => d.ToLowerInvariant().Contains("error cs") || d.StartsWith("error "));
    }
}
