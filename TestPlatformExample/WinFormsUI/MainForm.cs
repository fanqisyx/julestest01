using CorePlatform;
// using SamplePlugin; // No longer directly instantiating MyPlugin
using System.IO;    // For Path and Directory operations
// Ensure other necessary WinForms usings are present if this were a full file, e.g.
// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Data;
// using System.Drawing;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Windows.Forms;


namespace WinFormsUI
{
    public partial class MainForm : Form
    {
        private PluginManager _pluginManager;
        private ListBox lstLog;
        private Button btnRunTests;
        private Button btnLoadPlugin;


        public MainForm()
        {
            InitializeComponent(); // This method is in MainForm.Designer.cs
            _pluginManager = new PluginManager(LogMessage);
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
            // For WinForms, Application.StartupPath is common. AppDomain.CurrentDomain.BaseDirectory also works.
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
                    return; // Stop if directory can't be created
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
                LogMessage("No plugins are loaded. Click 'Load Plugins' first."); // Adjusted message
                return;
            }
            Task.Run(() => _pluginManager.RunPluginTests(LogMessage));
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
