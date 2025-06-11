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

// IronPython specific usings
using IronPython.Hosting;
using Microsoft.Scripting.Hosting; // For ScriptScope, ScriptEngine (IronPython's)
using Microsoft.Scripting;       // For SourceCodeKind, SyntaxErrorException

namespace CorePlatform
{
    public enum ScriptLanguage
    {
        CSharp,
        Python
    }

    public class ScriptEngine
    {
        private Action<string> _hostLogCallback;

        public ScriptEngine(Action<string> hostLogCallback)
        {
            _hostLogCallback = hostLogCallback;
        }

        private ScriptOptions CreateCSharpScriptOptions(List<string>? customNamespaces, List<string>? customAssemblyRefs)
        {
            _hostLogCallback?.Invoke("ScriptEngine: Creating C# ScriptOptions...");

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
            ScriptLanguage language, // New parameter
            PluginManager pluginManager,
            Action<string> uiLogCallbackForScriptHost,
            List<string>? customNamespaces, // Primarily for C#
            List<string>? customAssemblyRefs) // Primarily for C#
        {
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                return new ScriptExecutionResult { Success = false, ErrorMessage = "Script text cannot be empty." };
            }

            if (language == ScriptLanguage.CSharp)
            {
                ScriptOptions csharpOptions = CreateCSharpScriptOptions(customNamespaces, customAssemblyRefs);
                var actualScriptHostForCSharp = new ScriptingHost(pluginManager, uiLogCallbackForScriptHost);
                var scriptGlobals = new ScriptGlobals(actualScriptHostForCSharp);
                try
                {
                    _hostLogCallback?.Invoke("ScriptEngine: Executing C# script...");
                    var scriptState = await CSharpScript.RunAsync(scriptText, csharpOptions, globals: scriptGlobals, globalsType: typeof(ScriptGlobals));
                    _hostLogCallback?.Invoke("ScriptEngine: C# Script execution completed.");

                    if (scriptState.ReturnValue != null)
                    {
                        _hostLogCallback?.Invoke($"ScriptEngine: C# Script returned value: {scriptState.ReturnValue}");
                        return new ScriptExecutionResult { Success = true, ReturnValue = scriptState.ReturnValue };
                    }
                    return new ScriptExecutionResult { Success = true };
                }
                catch (CompilationErrorException cex)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: C# Compilation error: {cex.Message}");
                    var diagnostics = string.Join(Environment.NewLine, cex.Diagnostics.Select(d => d.ToString()));
                    _hostLogCallback?.Invoke($"C# Diagnostics:\n{diagnostics}");
                    return new ScriptExecutionResult { Success = false, ErrorMessage = "C# Script compilation failed.", CompilationErrors = cex.Diagnostics.Select(d => d.ToString()).ToList() };
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: C# Runtime error: {ex.Message}");
                    return new ScriptExecutionResult { Success = false, ErrorMessage = $"C# Script runtime error: {ex.Message}" };
                }
            }
            else if (language == ScriptLanguage.Python)
            {
                _hostLogCallback?.Invoke("ScriptEngine: Initializing IronPython engine...");
                var pyEngine = Python.CreateEngine();
                var pyScope = pyEngine.CreateScope();

                var actualScriptHostForPython = new ScriptingHost(pluginManager, uiLogCallbackForScriptHost);
                pyScope.SetVariable("Host", actualScriptHostForPython);

                // Example: Allow Python scripts to load CorePlatform types if needed via clr.AddReference
                // pyEngine.Runtime.LoadAssembly(typeof(CorePlatform.IPlugin).Assembly);

                try
                {
                    _hostLogCallback?.Invoke("ScriptEngine: Executing Python script...");
                    ScriptSource source = pyEngine.CreateScriptSourceFromString(scriptText, SourceCodeKind.Statements);
                    object? pythonResult = source.Execute(pyScope);
                    _hostLogCallback?.Invoke("ScriptEngine: Python script execution completed.");

                    if (pythonResult != null)
                    {
                        _hostLogCallback?.Invoke($"ScriptEngine: Python script returned value: {pythonResult}");
                        return new ScriptExecutionResult { Success = true, ReturnValue = pythonResult };
                    }
                    return new ScriptExecutionResult { Success = true };
                }
                catch (SyntaxErrorException pex) // IronPython specific for syntax errors
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Python syntax error: {pex.Message} (Line: {pex.Line}, Column: {pex.Column})");
                    return new ScriptExecutionResult { Success = false, ErrorMessage = $"Python syntax error: {pex.Message}", CompilationErrors = new List<string> { $"Line {pex.Line}, Column {pex.Column}: {pex.Message}" } };
                }
                catch (Exception ex) // General runtime errors
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Python runtime error: {ex.Message}");
                    string errorMessage = $"Python runtime error: {ex.GetType().Name}: {ex.Message}";
                    if (ex.InnerException != null) errorMessage += $" ---> {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

                    var eo = pyEngine.GetService<ExceptionOperations>();
                    if (eo != null) {
                        string pythonStackTrace = eo.FormatException(ex);
                        if (!string.IsNullOrEmpty(pythonStackTrace)) {
                             _hostLogCallback?.Invoke($"ScriptEngine: Python StackTrace:\n{pythonStackTrace}");
                             errorMessage += $"\nStackTrace:\n{pythonStackTrace}";
                        }
                    }
                    return new ScriptExecutionResult { Success = false, ErrorMessage = errorMessage };
                }
            }
            else
            {
                return new ScriptExecutionResult { Success = false, ErrorMessage = "Unsupported script language." };
            }
        }

        public ScriptCompilationCheckResult CheckSyntax(
            string scriptText,
            ScriptLanguage language, // New parameter
            List<string>? customNamespaces, // Primarily for C#
            List<string>? customAssemblyRefs) // Primarily for C#
        {
            var result = new ScriptCompilationCheckResult();
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                result.Success = true;
                return result;
            }

            if (language == ScriptLanguage.CSharp)
            {
                ScriptOptions csharpOptions = CreateCSharpScriptOptions(customNamespaces, customAssemblyRefs);
                try
                {
                    _hostLogCallback?.Invoke("ScriptEngine: Checking C# script syntax...");
                    var script = CSharpScript.Create(scriptText, csharpOptions, globalsType: typeof(ScriptGlobals));
                    var compilation = script.GetCompilation();
                    var diagnostics = compilation.GetDiagnostics();

                    result.Success = !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
                    foreach (var diagnostic in diagnostics)
                    {
                        result.Diagnostics.Add(diagnostic.ToString());
                    }

                    if (result.Success)
                    {
                        _hostLogCallback?.Invoke("ScriptEngine: C# Syntax check completed. No errors found.");
                    }
                    else
                    {
                        _hostLogCallback?.Invoke($"ScriptEngine: C# Syntax check completed. Errors found: {result.Diagnostics.Count(d => d.ToLowerInvariant().Contains("error"))}");
                    }
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Error during C# syntax check: {ex.Message}");
                    result.Diagnostics.Add($"Unexpected error during C# syntax check: {ex.Message}");
                    result.Success = false;
                }
            }
            else if (language == ScriptLanguage.Python)
            {
                _hostLogCallback?.Invoke("ScriptEngine: Initializing IronPython engine for syntax check...");
                var pyEngine = Python.CreateEngine();
                try
                {
                    _hostLogCallback?.Invoke("ScriptEngine: Checking Python script syntax...");
                    ScriptSource source = pyEngine.CreateScriptSourceFromString(scriptText, SourceCodeKind.Statements);
                    source.Compile(); // Compile will throw on syntax error
                    result.Success = true;
                    _hostLogCallback?.Invoke("ScriptEngine: Python Syntax check completed. No errors found.");
                }
                catch (SyntaxErrorException pex)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Python syntax error: {pex.Message} (Line: {pex.Line}, Column: {pex.Column})");
                    result.Diagnostics.Add($"Line {pex.Line}, Column {pex.Column}: {pex.Message}");
                    result.Success = false;
                }
                catch (Exception ex)
                {
                    _hostLogCallback?.Invoke($"ScriptEngine: Error during Python syntax check: {ex.Message}");
                    result.Diagnostics.Add($"Unexpected error during Python syntax check: {ex.Message}");
                    result.Success = false;
                }
            }
            else
            {
                result.Diagnostics.Add("Unsupported script language for syntax check.");
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
