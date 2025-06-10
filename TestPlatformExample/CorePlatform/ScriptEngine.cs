using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace CorePlatform
{
    public class ScriptEngine
    {
        // private ScriptOptions? _scriptOptions; // Removed field
        private Action<string> _hostLogCallback;

        public ScriptEngine(Action<string> hostLogCallback)
        {
            _hostLogCallback = hostLogCallback;
            // InitializeScriptOptions(); // Call removed from constructor
        }

        private ScriptOptions CreateScriptOptions(List<string>? customNamespaces, List<string>? customAssemblyRefs)
        {
            _hostLogCallback?.Invoke("ScriptEngine: Creating ScriptOptions...");
            var defaultReferences = new List<Assembly>
            {
                Assembly.GetAssembly(typeof(object))!,
                Assembly.GetAssembly(typeof(System.Linq.Enumerable))!,
                Assembly.GetAssembly(typeof(IPlugin))!,
                Assembly.GetAssembly(typeof(ScriptingHost))!
            };

            Assembly? formsAssembly = null;
            try
            {
                formsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(asm => asm.GetName().Name == "System.Windows.Forms");
                if (formsAssembly != null)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Found System.Windows.Forms assembly: {formsAssembly.FullName}");
                    defaultReferences.Add(formsAssembly);
                }
                else
                {
                    _hostLogCallback?.Invoke("ScriptEngine: System.Windows.Forms assembly not found in AppDomain. Will not be added to script references by default.");
                }
            }
            catch (Exception ex)
            {
                _hostLogCallback?.Invoke($"ScriptEngine: Error trying to locate System.Windows.Forms assembly: {ex.Message}");
            }

            var finalReferencesForOptions = new List<MetadataReference>();
            finalReferencesForOptions.AddRange(defaultReferences.Select(asm => MetadataReference.CreateFromFile(asm.Location)));

            // Add custom assembly references (by name or path)
            var customReferencePaths = new List<string>();
            if (customAssemblyRefs != null)
            {
                customReferencePaths.AddRange(customAssemblyRefs.Where(r => !string.IsNullOrWhiteSpace(r)));
                _hostLogCallback?.Invoke($"ScriptEngine: Adding {customReferencePaths.Count} custom assembly references by path/name.");
                // Note: Roslyn's AddReferences can also take assembly objects directly,
                // but for paths/names from user input, this is typical.
                // If these are GAC assemblies, just the name is fine. Otherwise, full paths are needed.
                // For simplicity, we're not resolving GAC names to paths here.
            }
            // Add custom references as MetadataReference objects if they are paths.
            // For simplicity, the ScriptOptions.AddReferences(IEnumerable<string>) overload is used later,
            // which can handle assembly display names or full paths.

            var imports = new List<string>
            {
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "CorePlatform"
            };
            if (formsAssembly != null)
            {
                imports.Add("System.Windows.Forms");
            }
            if (customNamespaces != null)
            {
                imports.AddRange(customNamespaces.Where(ns => !string.IsNullOrWhiteSpace(ns)));
                _hostLogCallback?.Invoke($"ScriptEngine: Adding {customNamespaces.Count(ns => !string.IsNullOrWhiteSpace(ns))} custom namespaces.");
            }

            var options = ScriptOptions.Default
                .AddReferences(defaultReferences) // Add loaded Assembly objects
                .AddReferences(customReferencePaths) // Add string names/paths for others
                .AddImports(imports.Distinct());

            _hostLogCallback?.Invoke("ScriptEngine: ScriptOptions created.");
            return options;
        }

        public async Task<ScriptExecutionResult> ExecuteScriptAsync(
            string scriptText,
            PluginManager pluginManager,
            Action<string> uiLogCallbackForScriptHost,
            List<string>? customNamespaces,
            List<string>? customAssemblyRefs)
        {
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                return new ScriptExecutionResult { Success = false, ErrorMessage = "Script text cannot be empty." };
            }

            ScriptOptions currentOptions = CreateScriptOptions(customNamespaces, customAssemblyRefs);
            var scriptHost = new ScriptingHost(pluginManager, uiLogCallbackForScriptHost);

            try
            {
                _hostLogCallback?.Invoke("ScriptEngine: Executing script with current options...");
                var scriptState = await CSharpScript.RunAsync(scriptText, currentOptions, globals: scriptHost, globalsType: typeof(ScriptingHost));
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

        public ScriptCompilationCheckResult CheckSyntax(
            string scriptText,
            List<string>? customNamespaces,
            List<string>? customAssemblyRefs)
        {
            var result = new ScriptCompilationCheckResult();
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                result.Success = true;
                return result;
            }

            ScriptOptions currentOptions = CreateScriptOptions(customNamespaces, customAssemblyRefs);

            try
            {
                _hostLogCallback?.Invoke("ScriptEngine: Checking script syntax with current options...");

                var script = CSharpScript.Create(scriptText, currentOptions, globalsType: typeof(ScriptingHost));
                var compilation = script.GetCompilation();
                var diagnostics = compilation.GetDiagnostics();

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
        public bool Success { get; set; }
        public List<string> Diagnostics { get; set; } = new List<string>();
        public bool HasErrors => Diagnostics.Any(d => d.ToLowerInvariant().Contains("error cs") || d.StartsWith("error "));
    }
}
