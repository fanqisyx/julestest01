using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO; // For Path
using System.Text; // For StringBuilder
using System.Text.RegularExpressions; // For Regex
using System.Net.Http; // For HttpClient
// using System.Threading.Tasks; // Already present
using System.Xml.Linq; // For XDocument
using System.Diagnostics; // For Stopwatch

namespace CorePlatform
{
    public class ScriptEngine
    {
        private Action<string> _hostLogCallback;

        public ScriptEngine(Action<string> hostLogCallback)
        {
            _hostLogCallback = hostLogCallback;
        }

        private ScriptOptions CreateScriptOptions(List<string>? customNamespaces, List<string>? customAssemblyRefs)
        {
            _hostLogCallback?.Invoke("ScriptEngine: Creating ScriptOptions...");

            var defaultAssemblies = new List<Assembly>
            {
                Assembly.GetAssembly(typeof(object))!,              // System.Private.CoreLib or mscorlib
                Assembly.GetAssembly(typeof(System.Linq.Enumerable))!, // System.Linq.Expressions / System.Core
                Assembly.GetAssembly(typeof(IPlugin))!,              // CorePlatform.dll
                Assembly.GetAssembly(typeof(ScriptingHost))!,        // CorePlatform.dll
                Assembly.GetAssembly(typeof(ScriptGlobals))!,        // CorePlatform.dll
                Assembly.GetAssembly(typeof(System.IO.Path))!,       // System.Runtime.Extensions / System.IO.FileSystem
                Assembly.GetAssembly(typeof(System.Text.StringBuilder))!,// System.Runtime / System.Text.Primitives
                Assembly.GetAssembly(typeof(System.Text.RegularExpressions.Regex))!, // System.Text.RegularExpressions
                Assembly.GetAssembly(typeof(System.Net.Http.HttpClient))!, // System.Net.Http
                Assembly.GetAssembly(typeof(System.Threading.Tasks.Task))!,// System.Threading.Tasks / System.Runtime
                Assembly.GetAssembly(typeof(System.Xml.Linq.XDocument))!, // System.Xml.Linq
                Assembly.GetAssembly(typeof(System.Diagnostics.Stopwatch))!// System.Diagnostics.TraceSource / System.Diagnostics.Stopwatch
            };

            Assembly? formsAssembly = null;
            try
            {
                formsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(asm => asm.GetName().Name == "System.Windows.Forms");
                if (formsAssembly != null)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Found System.Windows.Forms assembly: {formsAssembly.FullName}");
                    defaultAssemblies.Add(formsAssembly);
                }
                else
                {
                    _hostLogCallback?.Invoke("ScriptEngine: System.Windows.Forms assembly not found in AppDomain. WinForms types will not be available to scripts unless explicitly referenced by user.");
                }
            }
            catch (Exception ex)
            {
                _hostLogCallback?.Invoke($"ScriptEngine: Error trying to locate System.Windows.Forms assembly: {ex.Message}");
            }

            var customRefsList = new List<string>();
            if (customAssemblyRefs != null)
            {
                customRefsList.AddRange(customAssemblyRefs.Where(r => !string.IsNullOrWhiteSpace(r)));
                _hostLogCallback?.Invoke($"ScriptEngine: Adding {customRefsList.Count} custom assembly references by path/name.");
            }

            var defaultImports = new List<string>
            {
                "System",
                "System.IO",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Linq",
                "System.Collections.Generic",
                "System.Net.Http",
                "System.Threading.Tasks",
                "System.Xml.Linq",
                "System.Diagnostics",
                "CorePlatform"
            };

            if (formsAssembly != null)
            {
                defaultImports.Add("System.Windows.Forms");
            }

            var finalImports = new List<string>(defaultImports);
            if (customNamespaces != null)
            {
                finalImports.AddRange(customNamespaces.Where(ns => !string.IsNullOrWhiteSpace(ns)));
                _hostLogCallback?.Invoke($"ScriptEngine: Adding {customNamespaces.Count(ns => !string.IsNullOrWhiteSpace(ns))} custom namespaces.");
            }

            // Log final effective references and imports for debugging
            // _hostLogCallback?.Invoke($"ScriptEngine: Effective Assemblies: {string.Join(", ", defaultAssemblies.Select(a => a.GetName().Name).Concat(customRefsList).Distinct())}");
            // _hostLogCallback?.Invoke($"ScriptEngine: Effective Imports: {string.Join(", ", finalImports.Distinct())}");

            var options = ScriptOptions.Default
                .AddReferences(defaultAssemblies.Distinct())
                .AddReferences(customRefsList.Distinct())
                .AddImports(finalImports.Distinct());

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
            var actualScriptHost = new ScriptingHost(pluginManager, uiLogCallbackForScriptHost);
            var scriptGlobals = new ScriptGlobals(actualScriptHost);

            try
            {
                _hostLogCallback?.Invoke("ScriptEngine: Executing script with ScriptGlobals (Host property)...");
                var scriptState = await CSharpScript.RunAsync(scriptText, currentOptions, globals: scriptGlobals, globalsType: typeof(ScriptGlobals));
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

                var script = CSharpScript.Create(scriptText, currentOptions, globalsType: typeof(ScriptGlobals));
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
