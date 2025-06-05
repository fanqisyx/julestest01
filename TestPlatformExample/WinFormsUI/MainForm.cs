using CorePlatform;
using SamplePlugin; // For direct instantiation in this simple example

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
            InitializeComponent();
            _pluginManager = new PluginManager(LogMessage);
            // Manually add SamplePlugin for this example as reflection might be tricky
            // without placing plugins in a specific directory and loading assemblies from there.
            // _pluginManager.AddPlugin(new MyPlugin()); // Direct instantiation
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
            LogMessage("Attempting to load plugins...");
            // In a more complex app, you might scan a directory.
            // For this simple example, we'll explicitly add an instance of MyPlugin.
            // This bypasses complex assembly loading issues for a minimal example.
            IPlugin plugin = new MyPlugin();
            _pluginManager.AddPlugin(plugin); // Add the plugin
            LogMessage($"Plugin '{plugin.Name}' loaded and added to PluginManager.");
            RefreshPluginList();
        }

        private void btnRunTests_Click(object sender, EventArgs e)
        {
            LogMessage("Running tests for all loaded plugins...");
            if (!_pluginManager.GetPlugins().Any()) {
                LogMessage("No plugins are loaded. Click 'Load Sample Plugin' first.");
                return;
            }
            Task.Run(() => _pluginManager.RunPluginTests(LogMessage));
        }

        private void RefreshPluginList()
        {
            // Potentially update UI with loaded plugins, not implemented in detail for this basic version
            LogMessage($"Currently loaded plugins: {string.Join(", ", _pluginManager.GetPlugins().Select(p => p.Name))}");
        }
    }
}
