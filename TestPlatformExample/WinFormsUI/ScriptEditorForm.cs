using System;
using System.Windows.Forms;
using CorePlatform; // For ScriptEngine, PluginManager, ScriptExecutionResult, ScriptCompilationCheckResult
using FastColoredTextBoxNS;
using System.IO; // For Path, File, Directory
using System.Threading.Tasks; // For async/await and Task.Run
using System.Linq; // For .Any() on Diagnostics list
using System.Collections.Generic; // For List<string>
using System.ComponentModel; // For IContainer

namespace WinFormsUI
{
    public partial class ScriptEditorForm : Form
    {
        private System.ComponentModel.IContainer components = null; // Added field declaration

        private FastColoredTextBox fctbScriptEditor;
        private MenuStrip menuStripMain;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem scriptToolStripMenuItem;
        private ToolStripMenuItem runScriptToolStripMenuItem;
        private ToolStripMenuItem checkSyntaxToolStripMenuItem;
        private ToolStripMenuItem scriptSettingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem scriptingGuideToolStripMenuItem;
        private StatusStrip statusStripMain;
        private ToolStripStatusLabel statusLabel;

        private ScriptEngine _scriptEngine;
        private PluginManager _pluginManager;
        private Action<string> _mainFormLogCallback;

        private string _currentFilePath = string.Empty;
        private bool _isDirty = false;

        private List<string> _currentScriptNamespaces = new List<string>();
        private List<string> _currentScriptAssemblyRefs = new List<string>();

        public string? FinalScriptText { get; private set; }
        public bool WasClosedSuccessfully { get; private set; } = false;


        public ScriptEditorForm(ScriptEngine scriptEngine, PluginManager pluginManager, Action<string> mainFormLogCallback, string initialScriptText)
        {
            _scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            _mainFormLogCallback = mainFormLogCallback ?? throw new ArgumentNullException(nameof(mainFormLogCallback));

            // InitializeComponent(); // This would be called if using a .Designer.cs file
            SetupControlsManually(); // Our manual setup method

            this.Text = "Script Editor";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            if (this.fctbScriptEditor != null)
            {
                this.fctbScriptEditor.Text = initialScriptText ?? string.Empty;
                this._isDirty = false;
                UpdateFormTitle();
            }

            this.FormClosing += ScriptEditorForm_FormClosing;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) // Added standard Dispose method
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SetupControlsManually()
        {
            this.components = new System.ComponentModel.Container(); // This line is fine once 'components' is declared

            this.fctbScriptEditor = new FastColoredTextBox();
            this.fctbScriptEditor.Dock = DockStyle.Fill;
            this.fctbScriptEditor.Language = Language.CSharp;
            this.fctbScriptEditor.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // ... other FCTB properties ...
            this.fctbScriptEditor.Name = "fctbScriptEditor";
            this.fctbScriptEditor.TextChanged += (s, e) => { _isDirty = true; UpdateFormTitle(); };

            this.menuStripMain = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem("File");
            this.newToolStripMenuItem = new ToolStripMenuItem("New");
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            this.openToolStripMenuItem = new ToolStripMenuItem("Open...");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            this.saveToolStripMenuItem = new ToolStripMenuItem("Save");
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            this.saveAsToolStripMenuItem = new ToolStripMenuItem("Save As...");
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.closeToolStripMenuItem = new ToolStripMenuItem("Close");
            this.closeToolStripMenuItem.Click += (s,e) => this.Close();

            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.newToolStripMenuItem, this.openToolStripMenuItem, this.saveToolStripMenuItem, this.saveAsToolStripMenuItem,
                this.toolStripSeparator1, this.closeToolStripMenuItem
            });

            this.scriptToolStripMenuItem = new ToolStripMenuItem("Script");
            this.runScriptToolStripMenuItem = new ToolStripMenuItem("Run Script");
            this.runScriptToolStripMenuItem.Click += new System.EventHandler(this.runScriptToolStripMenuItem_Click);
            this.checkSyntaxToolStripMenuItem = new ToolStripMenuItem("Check Syntax");
            this.checkSyntaxToolStripMenuItem.Click += new System.EventHandler(this.checkSyntaxToolStripMenuItem_Click);
            this.scriptSettingsToolStripMenuItem = new ToolStripMenuItem("Script Settings...");
            this.scriptSettingsToolStripMenuItem.Click += new System.EventHandler(this.scriptSettingsToolStripMenuItem_Click);

            this.scriptToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                this.runScriptToolStripMenuItem,
                this.checkSyntaxToolStripMenuItem,
                new ToolStripSeparator(),
                this.scriptSettingsToolStripMenuItem
            });

            this.helpToolStripMenuItem = new ToolStripMenuItem("Help");
            this.scriptingGuideToolStripMenuItem = new ToolStripMenuItem("Scripting Guide...");
            this.scriptingGuideToolStripMenuItem.Click += new System.EventHandler(this.scriptingGuideToolStripMenuItem_Click);
            this.helpToolStripMenuItem.DropDownItems.Add(this.scriptingGuideToolStripMenuItem);

            this.menuStripMain.Items.AddRange(new ToolStripItem[] { this.fileToolStripMenuItem, this.scriptToolStripMenuItem, this.helpToolStripMenuItem });

            this.statusStripMain = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel("Ready");
            this.statusStripMain.Items.Add(this.statusLabel);

            this.Controls.Add(this.fctbScriptEditor);
            this.Controls.Add(this.menuStripMain);
            this.Controls.Add(this.statusStripMain);

            this.MainMenuStrip = this.menuStripMain;
        }

        private void UpdateFormTitle()
        {
            string title = "Script Editor";
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                title += " - " + Path.GetFileName(_currentFilePath);
            }
            if (_isDirty)
            {
                title += "*";
            }
            this.Text = title;
        }

        private DialogResult CheckUnsavedChanges()
        {
            if (!_isDirty) return DialogResult.Yes;
            DialogResult res = MessageBox.Show(this, "The current script has unsaved changes. Do you want to save them?", "Unsaved Changes",  MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes) return SaveScript() ? DialogResult.Yes : DialogResult.Cancel;
            return res;
        }

        private void ScriptEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult proceedAction = CheckUnsavedChanges();
            if (proceedAction == DialogResult.Cancel) { e.Cancel = true; WasClosedSuccessfully = false; }
            else { e.Cancel = false; FinalScriptText = this.fctbScriptEditor.Text; WasClosedSuccessfully = true; }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckUnsavedChanges() == DialogResult.Cancel) return;
            this.fctbScriptEditor.Text = string.Empty;
            this._currentFilePath = string.Empty; this._isDirty = false; UpdateFormTitle();
            this.statusLabel.Text = "New script created. Ready.";
            _mainFormLogCallback?.Invoke("ScriptEditor: New script created.");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckUnsavedChanges() == DialogResult.Cancel) { _mainFormLogCallback?.Invoke("ScriptEditor: 'Open Script' cancelled."); return; }
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "C# Scripts (*.cs)|*.cs|CSX Scripts (*.csx)|*.csx|Text Files (*.txt)|*.txt|All Files (*.*)|*.*", DefaultExt = "cs", Title = "Open Script File...", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK) {
                    try {
                        this.fctbScriptEditor.Text = File.ReadAllText(ofd.FileName);
                        _currentFilePath = ofd.FileName; _isDirty = false; UpdateFormTitle();
                        this.statusLabel.Text = $"Script '{Path.GetFileName(_currentFilePath)}' opened.";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Script '{_currentFilePath}' opened.");
                    } catch (Exception ex) {
                        this.statusLabel.Text = "Error opening script.";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Error opening '{ofd.FileName}': {ex.Message}");
                        MessageBox.Show(this, $"Error opening script: {ex.Message}", "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                } else { _mainFormLogCallback?.Invoke("ScriptEditor: 'Open Script' dialog cancelled."); }
            }
        }

        private bool SaveScriptAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "C# Scripts (*.cs)|*.cs|Text Files (*.txt)|*.txt|All Files (*.*)|*.*", DefaultExt = "cs", Title = "Save Script As..." }) {
                sfd.FileName = !string.IsNullOrEmpty(_currentFilePath) ? Path.GetFileName(_currentFilePath) : "NewScript.cs";
                sfd.InitialDirectory = !string.IsNullOrEmpty(_currentFilePath) ? Path.GetDirectoryName(_currentFilePath) : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (sfd.ShowDialog(this) == DialogResult.OK) {
                    try {
                        File.WriteAllText(sfd.FileName, this.fctbScriptEditor.Text);
                        _currentFilePath = sfd.FileName; _isDirty = false; UpdateFormTitle();
                        this.statusLabel.Text = $"Script saved: {Path.GetFileName(_currentFilePath)}";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Script saved to '{_currentFilePath}'.");
                        return true;
                    } catch (Exception ex) {
                        this.statusLabel.Text = "Error saving script.";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Error saving to '{sfd.FileName}': {ex.Message}");
                        MessageBox.Show(this, $"Error saving: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                } return false;
            }
        }
        private bool SaveScript() => !string.IsNullOrEmpty(_currentFilePath) ? SaveCurrentFile() : SaveScriptAs();
        private bool SaveCurrentFile() {
            try {
                File.WriteAllText(_currentFilePath, this.fctbScriptEditor.Text);
                _isDirty = false; UpdateFormTitle();
                this.statusLabel.Text = $"Script saved: {Path.GetFileName(_currentFilePath)}";
                _mainFormLogCallback?.Invoke($"ScriptEditor: Script saved to '{_currentFilePath}'.");
                return true;
            } catch (Exception ex) {
                this.statusLabel.Text = "Error saving script.";
                _mainFormLogCallback?.Invoke($"ScriptEditor: Error saving to '{_currentFilePath}': {ex.Message}");
                MessageBox.Show(this, $"Error saving: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) { SaveScript(); }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) { SaveScriptAs(); }

        private async void runScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scriptText = this.fctbScriptEditor.Text;
            if (string.IsNullOrWhiteSpace(scriptText)) { this.statusLabel.Text = "Script is empty."; _mainFormLogCallback?.Invoke("ScriptEditor: Run: Empty script."); return; }

            this.statusLabel.Text = "Executing script...";
            _mainFormLogCallback?.Invoke($"ScriptEditor: Running script (settings: {this._currentScriptNamespaces.Count} ns, {this._currentScriptAssemblyRefs.Count} refs)...");

            ScriptExecutionResult result = await _scriptEngine.ExecuteScriptAsync(
                scriptText,
                _pluginManager,
                _mainFormLogCallback,
                _currentScriptNamespaces,
                _currentScriptAssemblyRefs
            );
            this.statusLabel.Text = result.Success ? "Script execution successful." : "Script execution failed. See main log.";
        }

        private async void checkSyntaxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scriptText = this.fctbScriptEditor.Text;
            if (string.IsNullOrWhiteSpace(scriptText)) { this.statusLabel.Text = "Script is empty."; _mainFormLogCallback?.Invoke("ScriptEditor: Check Syntax: Empty script."); return; }

            this.statusLabel.Text = "Checking syntax...";
            _mainFormLogCallback?.Invoke($"ScriptEditor: Checking syntax (settings: {this._currentScriptNamespaces.Count} ns, {this._currentScriptAssemblyRefs.Count} refs)...");

            ScriptCompilationCheckResult result = await Task.Run(() => _scriptEngine.CheckSyntax(
                scriptText,
                _currentScriptNamespaces,
                _currentScriptAssemblyRefs
            ));

            if (result.Success) {
                this.statusLabel.Text = "Syntax check: OK." + (result.Diagnostics.Any() ? " (Warnings present)" : "");
                _mainFormLogCallback?.Invoke("ScriptEditor: Syntax check: OK.");
                if (result.Diagnostics.Any()) { _mainFormLogCallback?.Invoke("Diagnostics (Warnings/Info):"); result.Diagnostics.ForEach(d => _mainFormLogCallback?.Invoke($"  DIAG: {d}")); }
            } else {
                this.statusLabel.Text = "Syntax check: Errors found. See main log.";
                _mainFormLogCallback?.Invoke("ScriptEditor: Syntax check: Errors found.");
                result.Diagnostics.ForEach(err => _mainFormLogCallback?.Invoke($"  SYNTAX ERR: {err}"));
            }
        }

        private void scriptSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ScriptSettingsForm settingsForm = new ScriptSettingsForm(
                new List<string>(_currentScriptNamespaces),
                new List<string>(_currentScriptAssemblyRefs)
            ))
            {
                settingsForm.Owner = this;
                if (settingsForm.ShowDialog(this) == DialogResult.OK)
                {
                    _currentScriptNamespaces = settingsForm.AdditionalNamespaces;
                    _currentScriptAssemblyRefs = settingsForm.AdditionalAssemblyReferences;
                    string logMsg = "Script settings updated." +
                                    $" Namespaces: [{string.Join(", ", _currentScriptNamespaces)}]." +
                                    $" Assemblies: [{string.Join(", ", _currentScriptAssemblyRefs)}].";
                    _mainFormLogCallback?.Invoke($"ScriptEditor: {logMsg}");
                    this.statusLabel.Text = "Script settings updated.";
                    _mainFormLogCallback?.Invoke("ScriptEditor: NOTE - ScriptEngine now uses these settings for next Run/Check Syntax.");
                }
                else
                {
                    _mainFormLogCallback?.Invoke("ScriptEditor: Script settings update cancelled.");
                    this.statusLabel.Text = "Script settings update cancelled.";
                }
            }
        }

        private void scriptingGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _mainFormLogCallback?.Invoke("ScriptEditor: Opening Scripting Help & Examples window...");
            HelpForm helpForm = new HelpForm();
            helpForm.Owner = this;
            helpForm.Show();
        }

        // Removed empty InitializeComponent() as it's not called and not standard for fully manual forms
        // private void InitializeComponent() { /* Manual setup done in SetupControlsManually */ }
    }
}
