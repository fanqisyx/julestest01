using CorePlatform;
using System.IO;    // For Path, Directory, File, Environment
using System;       // For EventArgs, DateTime, Action, StringSplitOptions etc.
using System.Linq;  // For Enumerable.Any(), Enumerable.Select()
using System.Threading.Tasks; // For Task
using System.Windows.Forms; // For Form, ListBox, Button, Application, MessageBox etc.
using FastColoredTextBoxNS; // Added for FastColoredTextBox control
using System.Diagnostics; // For Debug.WriteLine

// Note: Some of these might be covered by ImplicitUsings in net8.0, but explicit is clearer.

namespace WinFormsUI
{
    public partial class MainForm : Form
    {
        private PluginManager _pluginManager;
        private ScriptEngine _scriptEngine;

        // Static members for file logging
        private static readonly string _logFilePath = Path.Combine(Application.StartupPath, "TestPlatformExample.log");
        private static readonly object _logFileLock = new object();

        public MainForm()
        {
            InitializeComponent(); // This method is in MainForm.Designer.cs
            _pluginManager = new PluginManager(LogMessage);
            _scriptEngine = new ScriptEngine(LogMessage); // ScriptEngine's own logs also go to LogMessage
            LogMessage("Application started. Logging initialized."); // Initial log message
        }

        private void LogMessage(string originalMessage) // Parameter is the raw message
        {
            string timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {originalMessage}";

            if (lstLog.InvokeRequired)
            {
                lstLog.BeginInvoke(new Action(() =>
                {
                    lstLog.Items.Add(timestampedMessage);
                    if (lstLog.Items.Count > 0)
                    {
                        lstLog.TopIndex = lstLog.Items.Count - 1;
                    }
                }));
            }
            else
            {
                lstLog.Items.Add(timestampedMessage);
                if (lstLog.Items.Count > 0)
                {
                    lstLog.TopIndex = lstLog.Items.Count - 1;
                }
            }

            try
            {
                lock (_logFileLock)
                {
                    File.AppendAllText(_logFilePath, timestampedMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to write to log file '{_logFilePath}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file '{_logFilePath}': {ex.Message}");
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
                LogMessage("Script: Input is empty.");
                return;
            }

            LogMessage("Script: Executing script...");
            if (_pluginManager == null) {
                LogMessage("Error: PluginManager not initialized.");
                return;
            }
            if (_scriptEngine == null) {
                LogMessage("Error: ScriptEngine not initialized.");
                return;
            }

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
                LogMessage("Script: Execution failed.");

                if (result.CompilationErrors != null && result.CompilationErrors.Any())
                {
                    LogMessage("Script: Compilation Errors Reported by Engine:");
                    foreach (string error in result.CompilationErrors)
                    {
                        LogMessage($"  {error}");
                    }
                }

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    bool compilationAlreadyDetailed = result.CompilationErrors != null && result.CompilationErrors.Any();
                    if (!compilationAlreadyDetailed || !result.ErrorMessage.ToLowerInvariant().Contains("compilation failed"))
                    {
                         LogMessage($"Script: Engine Message = {result.ErrorMessage}");
                    }
                }
            }
        }

        private void btnOpenScriptEditor_Click(object sender, EventArgs e)
        {
            if (_scriptEngine == null || _pluginManager == null)
            {
                LogMessage("Error: Core components (ScriptEngine/PluginManager) not ready for Script Editor.");
                MessageBox.Show("Scripting components are not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ScriptEditorForm scriptEditor = new ScriptEditorForm(_scriptEngine, _pluginManager, LogMessage);
            scriptEditor.Show();
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
