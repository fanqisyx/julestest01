using CorePlatform;
using System.IO;    // For Path and Directory operations
using System;       // For EventArgs, DateTime, Action, StringSplitOptions etc.
using System.Linq;  // For Enumerable.Any(), Enumerable.Select()
using System.Threading.Tasks; // For Task
using System.Windows.Forms; // For Form, ListBox, Button, Application, MessageBox etc.
using FastColoredTextBoxNS; // Added for FastColoredTextBox control

// Note: Some of these might be covered by ImplicitUsings in net8.0, but explicit is clearer.

namespace WinFormsUI
{
    public partial class MainForm : Form
    {
        private PluginManager _pluginManager;
        private ScriptEngine _scriptEngine;
        // Control fields are now solely in MainForm.Designer.cs

        public MainForm()
        {
            InitializeComponent(); // This method is in MainForm.Designer.cs
            _pluginManager = new PluginManager(LogMessage);
            _scriptEngine = new ScriptEngine(LogMessage); // ScriptEngine's own logs also go to LogMessage
        }

        private void LogMessage(string message)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action<string>(LogMessage), message);
            }
            else
            {
                lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                lstLog.TopIndex = lstLog.Items.Count - 1; // Auto-scroll
            }
        }

        private void btnLoadPlugin_Click(object sender, EventArgs e)
        {
            LogMessage("Attempting to discover and load plugins...");

            string pluginDirName = "Plugins";
            string baseDirectory = Application.StartupPath;
            string pluginFolderPath = Path.Combine(baseDirectory, pluginDirName);

            LogMessage($"Plugin directory target: {pluginFolderPath}");

            if (!Directory.Exists(pluginFolderPath))
            {
                LogMessage($"Plugin directory '{pluginFolderPath}' does not exist. Attempting to create it.");
                try
                {
                    Directory.CreateDirectory(pluginFolderPath);
                    LogMessage($"Successfully created plugin directory: {pluginFolderPath}");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error creating plugin directory '{pluginFolderPath}': {ex.Message}. Please create it manually.");
                    return;
                }
            }
            else
            {
                LogMessage($"Plugin directory already exists: {pluginFolderPath}");
            }

            _pluginManager.DiscoverPlugins(pluginFolderPath);
            RefreshPluginList();
        }

        private void btnRunTests_Click(object sender, EventArgs e)
        {
            LogMessage("Running tests for all loaded plugins...");
            if (!_pluginManager.GetPlugins().Any()) {
                LogMessage("No plugins are loaded. Click 'Load Plugins' first.");
                return;
            }
            Task.Run(() => _pluginManager.RunPluginTests(LogMessage));
        }

        private async void btnRunScript_Click(object sender, EventArgs e)
        {
            string scriptText = fctbScriptInput.Text;
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                LogMessage("Script: Input is empty."); // Prefixed for clarity
                return;
            }

            LogMessage("Script: Executing script..."); // Prefixed for clarity
            if (_pluginManager == null) {
                LogMessage("Error: PluginManager not initialized.");
                return;
            }
            if (_scriptEngine == null) {
                LogMessage("Error: ScriptEngine not initialized.");
                return;
            }

            // Pass LogMessage to be used by the ScriptingHost instance for this script execution
            ScriptExecutionResult result = await _scriptEngine.ExecuteScriptAsync(scriptText, _pluginManager, LogMessage);

            if (result.Success)
            {
                LogMessage("Script: Executed successfully.");
                if (result.ReturnValue != null)
                {
                    LogMessage($"Script: ReturnValue = {result.ReturnValue}");
                }
            }
            else
            {
                LogMessage("Script: Execution failed."); // General script failure message

                if (result.CompilationErrors != null && result.CompilationErrors.Any())
                {
                    LogMessage("Script: Compilation Errors Reported by Engine:");
                    foreach (string error in result.CompilationErrors)
                    {
                        // Each 'error' string from Roslyn often contains location, error code, and message.
                        LogMessage($"  {error}"); // Indent for readability
                    }
                }
                // The ScriptEngine's own log (passed via _hostLogCallback to ScriptEngine constructor)
                // might have already logged "ScriptEngine: Compilation error: ..." or "ScriptEngine: Runtime error: ..."
                // The result.ErrorMessage from ScriptEngine might be a more general summary.
                // Ensure ErrorMessage is logged if it provides different info than compilation errors
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    // Avoid duplicating "compilation failed" if already detailed by CompilationErrors list.
                    bool compilationAlreadyDetailed = result.CompilationErrors != null && result.CompilationErrors.Any();
                    if (!compilationAlreadyDetailed || !result.ErrorMessage.ToLowerInvariant().Contains("compilation failed"))
                    {
                         LogMessage($"Script: Engine Message = {result.ErrorMessage}");
                    }
                }
            }
        }

        private void RefreshPluginList()
        {
            var loadedPlugins = _pluginManager.GetPlugins();
            if (loadedPlugins.Any())
            {
                LogMessage($"Currently loaded plugins: {string.Join(", ", loadedPlugins.Select(p => p.Name))}");
            }
            else
            {
                LogMessage("No plugins are currently loaded.");
            }
        }
    }
}
