using System;
using System.Windows.Forms;
using CorePlatform; // For ScriptEngine, PluginManager, ScriptExecutionResult, ScriptCompilationCheckResult
using FastColoredTextBoxNS;
using System.IO; // For Path, File, Directory
using System.Threading.Tasks; // For async/await and Task.Run
using System.Linq; // For .Any() on Diagnostics list

namespace WinFormsUI
{
    public partial class ScriptEditorForm : Form
    {
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
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem scriptingGuideToolStripMenuItem;
        private StatusStrip statusStripMain;
        private ToolStripStatusLabel statusLabel;

        private ScriptEngine _scriptEngine;
        private PluginManager _pluginManager;
        private Action<string> _mainFormLogCallback;

        private string _currentFilePath = string.Empty;
        private bool _isDirty = false;

        // Public properties for MainForm to access after dialog closes
        public string? FinalScriptText { get; private set; }
        public bool WasClosedSuccessfully { get; private set; } = false;


        public ScriptEditorForm(ScriptEngine scriptEngine, PluginManager pluginManager, Action<string> mainFormLogCallback, string initialScriptText)
        {
            _scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            _mainFormLogCallback = mainFormLogCallback ?? throw new ArgumentNullException(nameof(mainFormLogCallback));

            SetupControlsManually();
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

        private void SetupControlsManually()
        {
            this.components = new System.ComponentModel.Container();

            this.fctbScriptEditor = new FastColoredTextBox();
            this.fctbScriptEditor.Dock = DockStyle.Fill;
            this.fctbScriptEditor.Language = Language.CSharp;
            this.fctbScriptEditor.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fctbScriptEditor.LineNumberColor = System.Drawing.Color.Teal;
            this.fctbScriptEditor.ShowLineNumbers = true;
            this.fctbScriptEditor.IndentBackColor = System.Drawing.Color.WhiteSmoke;
            this.fctbScriptEditor.ServiceLinesColor = System.Drawing.Color.Silver;
            this.fctbScriptEditor.CaretColor = System.Drawing.Color.Red;
            this.fctbScriptEditor.ShowFoldingLines = true;
            this.fctbScriptEditor.AutoCompleteBrackets = true;
            this.fctbScriptEditor.AutoScrollMinSize = new System.Drawing.Size(2, 14);
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
            this.scriptToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.runScriptToolStripMenuItem, this.checkSyntaxToolStripMenuItem });

            this.helpToolStripMenuItem = new ToolStripMenuItem("Help");
            this.scriptingGuideToolStripMenuItem = new ToolStripMenuItem("Scripting Guide...");
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
            if (!_isDirty)
            {
                return DialogResult.Yes;
            }

            DialogResult res = MessageBox.Show(this,
                "The current script has unsaved changes. Do you want to save them?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (res == DialogResult.Yes)
            {
                if (SaveScript())
                {
                    return DialogResult.Yes;
                }
                else
                {
                    return DialogResult.Cancel;
                }
            }
            return res;
        }

        private void ScriptEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult proceedAction = CheckUnsavedChanges();

            if (proceedAction == DialogResult.Cancel)
            {
                e.Cancel = true;
                WasClosedSuccessfully = false;
            }
            else
            {
                e.Cancel = false;
                FinalScriptText = this.fctbScriptEditor.Text;
                WasClosedSuccessfully = true;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult proceedAction = CheckUnsavedChanges();
            if (proceedAction == DialogResult.Cancel)
            {
                return;
            }

            this.fctbScriptEditor.Text = string.Empty;
            this._currentFilePath = string.Empty;
            this._isDirty = false;
            UpdateFormTitle();
            this.statusLabel.Text = "New script created. Ready.";
            _mainFormLogCallback?.Invoke("ScriptEditor: New script created.");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult proceedAction = CheckUnsavedChanges();
            if (proceedAction == DialogResult.Cancel)
            {
                _mainFormLogCallback?.Invoke("ScriptEditor: 'Open Script' cancelled due to unsaved changes prompt.");
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "C# Scripts (*.cs)|*.cs|CSX Scripts (*.csx)|*.csx|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                ofd.DefaultExt = "cs";
                ofd.Title = "Open Script File...";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(ofd.FileName);
                        this.fctbScriptEditor.Text = fileContent;
                        _currentFilePath = ofd.FileName;
                        _isDirty = false;
                        UpdateFormTitle();
                        this.statusLabel.Text = $"Script '{Path.GetFileName(_currentFilePath)}' opened successfully.";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Script '{_currentFilePath}' opened.");
                    }
                    catch (Exception ex)
                    {
                        this.statusLabel.Text = "Error opening script.";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Error opening script '{ofd.FileName}': {ex.Message}");
                        MessageBox.Show(this, $"Error opening script file: {ex.Message}", "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    _mainFormLogCallback?.Invoke("ScriptEditor: 'Open Script' dialog cancelled by user.");
                }
            }
        }

        private bool SaveScriptAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "C# Scripts (*.cs)|*.cs|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                sfd.DefaultExt = "cs";
                sfd.Title = "Save Script As...";
                if (!string.IsNullOrEmpty(_currentFilePath))
                {
                    sfd.FileName = Path.GetFileName(_currentFilePath);
                    sfd.InitialDirectory = Path.GetDirectoryName(_currentFilePath);
                }
                else
                {
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, this.fctbScriptEditor.Text);
                        _currentFilePath = sfd.FileName;
                        _isDirty = false;
                        UpdateFormTitle();
                        this.statusLabel.Text = $"Script saved to {Path.GetFileName(_currentFilePath)}";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Script saved to '{_currentFilePath}'.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        this.statusLabel.Text = "Error saving script.";
                        _mainFormLogCallback?.Invoke($"ScriptEditor: Error saving script to '{sfd.FileName}': {ex.Message}");
                        MessageBox.Show(this, $"Error saving script: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                return false;
            }
        }

        private bool SaveScript()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                return SaveScriptAs();
            }
            else
            {
                try
                {
                    File.WriteAllText(_currentFilePath, this.fctbScriptEditor.Text);
                    _isDirty = false;
                    UpdateFormTitle();
                    this.statusLabel.Text = $"Script saved to {Path.GetFileName(_currentFilePath)}";
                    _mainFormLogCallback?.Invoke($"ScriptEditor: Script saved to '{_currentFilePath}'.");
                    return true;
                }
                catch (Exception ex)
                {
                    this.statusLabel.Text = "Error saving script.";
                    _mainFormLogCallback?.Invoke($"ScriptEditor: Error saving script to '{_currentFilePath}': {ex.Message}");
                    MessageBox.Show(this, $"Error saving script: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveScript();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveScriptAs();
        }

        private async void runScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scriptText = this.fctbScriptEditor.Text;
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                this.statusLabel.Text = "Script is empty. Nothing to run.";
                _mainFormLogCallback?.Invoke("ScriptEditor: Attempted to run an empty script.");
                return;
            }

            this.statusLabel.Text = "Executing script...";
            string scriptExcerpt = scriptText.Length > 80 ? scriptText.Substring(0, 80) : scriptText;
            _mainFormLogCallback?.Invoke($"ScriptEditor: Running script starting with: \"{scriptExcerpt.Replace("\n", " ").Replace("\r", "")}...\"");

            ScriptExecutionResult result = await _scriptEngine.ExecuteScriptAsync(scriptText, _pluginManager, _mainFormLogCallback);

            if (result.Success)
            {
                this.statusLabel.Text = "Script execution finished successfully.";
            }
            else
            {
                this.statusLabel.Text = "Script execution failed. Check main log for details.";
            }
        }

        private async void checkSyntaxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scriptText = this.fctbScriptEditor.Text;
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                this.statusLabel.Text = "Script is empty. Nothing to check.";
                _mainFormLogCallback?.Invoke("ScriptEditor: Check Syntax: Script is empty.");
                return;
            }

            this.statusLabel.Text = "Checking syntax...";
            string scriptExcerpt = scriptText.Length > 80 ? scriptText.Substring(0, 80) : scriptText;
            _mainFormLogCallback?.Invoke($"ScriptEditor: Checking syntax for script starting with: \"{scriptExcerpt.Replace("\n", " ").Replace("\r", "")}...\"");

            ScriptCompilationCheckResult result = await Task.Run(() => _scriptEngine.CheckSyntax(scriptText));

            if (result.Success)
            {
                this.statusLabel.Text = "Syntax check completed: OK.";
                _mainFormLogCallback?.Invoke("ScriptEditor: Check Syntax: OK.");
                if (result.Diagnostics.Any())
                {
                    _mainFormLogCallback?.Invoke("ScriptEditor: Check Syntax: Diagnostics reported by Engine (Warnings/Info):");
                    foreach (string diag in result.Diagnostics)
                    {
                        _mainFormLogCallback?.Invoke($"  DIAG: {diag}");
                    }
                }
            }
            else
            {
                this.statusLabel.Text = "Syntax check completed: Errors found. See main log.";
                _mainFormLogCallback?.Invoke("ScriptEditor: Check Syntax: Errors Reported by Engine:");
                foreach (string error in result.Diagnostics)
                {
                    _mainFormLogCallback?.Invoke($"  SYNTAX: {error}");
                }
            }
        }

        private void InitializeComponent()
        {
            // All control initialization is done in SetupControlsManually for this subtask
        }
    }
}
