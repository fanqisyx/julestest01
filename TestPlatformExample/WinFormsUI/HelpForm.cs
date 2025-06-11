using System;
using System.Drawing;
using System.Windows.Forms;
using FastColoredTextBoxNS; // For fctbExampleViewer
using System.IO; // For File and Path operations
using System.ComponentModel; // For IContainer
using System.Collections.Generic; // For Tuple

namespace WinFormsUI
{
    public partial class HelpForm : Form
    {
        private System.ComponentModel.IContainer components = null;

        private SplitContainer splitContainerMain;
        private RichTextBox rtbGuideViewer;

        private SplitContainer splitContainerExamples;
        private TreeView tvExamples;
        private FastColoredTextBox fctbExampleViewer;
        private Button btnCopyScript;

        private static readonly Tuple<string, string>[] ExampleScripts;

        static HelpForm()
        {
            var tempScriptsList = new List<Tuple<string, string>>();
            string baseDir = Application.StartupPath;
            string tempScriptsDir = Path.Combine(baseDir, "temp_scripts");

            var scriptFiles = new[] {
                new { Title = "1. Basic Logging", FileName = "example1.cs.txt" },
                new { Title = "2. List Loaded Plugins", FileName = "example2.cs.txt" },
                new { Title = "3. Execute Command on SamplePlugin", FileName = "example3.cs.txt" },
                new { Title = "4. Handle Plugin Not Found", FileName = "example4.cs.txt" },
                new { Title = "5. Use MessageBox (WinForms)", FileName = "example5.cs.txt" }
            };

            foreach (var scriptFile in scriptFiles)
            {
                string filePath = Path.Combine(tempScriptsDir, scriptFile.FileName);
                try
                {
                    if (File.Exists(filePath))
                    {
                        string scriptCode = File.ReadAllText(filePath);
                        tempScriptsList.Add(Tuple.Create(scriptFile.Title, scriptCode));
                    }
                    else
                    {
                        tempScriptsList.Add(Tuple.Create(scriptFile.Title + " (Error: File Not Found)", $"// Error: Could not load {scriptFile.FileName} from {tempScriptsDir}"));
                        Console.Error.WriteLine($"HelpForm Error: Example script file not found: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    tempScriptsList.Add(Tuple.Create(scriptFile.Title + $" (Error: {ex.GetType().Name})", $"// Error loading {scriptFile.FileName}: {ex.Message}"));
                    Console.Error.WriteLine($"HelpForm Error: Exception loading script file {filePath}: {ex.Message}");
                }
            }
            ExampleScripts = tempScriptsList.ToArray();
        }

        public HelpForm()
        {
            InitializeComponentManual();
            this.Text = "Scripting Help & Examples";
            this.Size = new System.Drawing.Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            LoadGuideContent();
            PopulateExampleScripts();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponentManual()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.SuspendLayout();

            this.splitContainerExamples = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerExamples)).BeginInit();
            this.splitContainerExamples.SuspendLayout();

            this.rtbGuideViewer = new System.Windows.Forms.RichTextBox();
            this.rtbGuideViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbGuideViewer.Name = "rtbGuideViewer";
            this.rtbGuideViewer.ReadOnly = true;
            this.rtbGuideViewer.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.rtbGuideViewer.BorderStyle = BorderStyle.FixedSingle;

            this.tvExamples = new System.Windows.Forms.TreeView();
            this.tvExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvExamples.Name = "tvExamples";
            this.tvExamples.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tvExamples.BorderStyle = BorderStyle.FixedSingle;
            this.tvExamples.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvExamples_AfterSelect);

            this.fctbExampleViewer = new FastColoredTextBox();
            this.fctbExampleViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fctbExampleViewer.Name = "fctbExampleViewer";
            this.fctbExampleViewer.ReadOnly = true;
            this.fctbExampleViewer.Language = Language.CSharp;
            this.fctbExampleViewer.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.fctbExampleViewer.ShowLineNumbers = true;
            this.fctbExampleViewer.BorderStyle = BorderStyle.FixedSingle;

            this.btnCopyScript = new System.Windows.Forms.Button();
            this.btnCopyScript.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCopyScript.Name = "btnCopyScript";
            this.btnCopyScript.Text = "Copy Script to Clipboard";
            this.btnCopyScript.Size = new System.Drawing.Size(100, 28);
            this.btnCopyScript.UseVisualStyleBackColor = true;
            this.btnCopyScript.Click += new System.EventHandler(this.btnCopyScript_Click);

            this.splitContainerExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerExamples.Name = "splitContainerExamples";
            this.splitContainerExamples.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitContainerExamples.SplitterDistance = 250;
            this.splitContainerExamples.Panel1.Controls.Add(this.tvExamples);
            this.splitContainerExamples.Panel2.Controls.Add(this.fctbExampleViewer);
            this.splitContainerExamples.Panel2.Controls.Add(this.btnCopyScript);

            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainerMain.SplitterDistance = 300;
            this.splitContainerMain.Panel1.Controls.Add(this.rtbGuideViewer);
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerExamples);

            this.Controls.Add(this.splitContainerMain);

            this.splitContainerExamples.Panel1.ResumeLayout(false);
            this.splitContainerExamples.Panel2.ResumeLayout(false);
            this.splitContainerExamples.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerExamples)).EndInit();

            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            this.splitContainerMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadGuideContent()
        {
            string guideFileName = "SCRIPTING_PLATFORM_GUIDE.md";
            string guideFilePath = "";
            bool fileFound = false;
            string path1 = Path.Combine(Application.StartupPath, guideFileName);
            string path2 = Path.Combine(Application.StartupPath, "..", "..", "..", guideFileName);
            string path3 = Path.Combine(Application.StartupPath, "..", guideFileName);

            if (File.Exists(path1)) { guideFilePath = path1; fileFound = true; }
            else if (File.Exists(path2)) { guideFilePath = path2; fileFound = true; }
            else if (File.Exists(path3)) { guideFilePath = path3; fileFound = true; }

            try {
                if (fileFound) { this.rtbGuideViewer.Text = File.ReadAllText(guideFilePath); }
                else { this.rtbGuideViewer.Text = $"Error: Could not find '{guideFileName}'.\nChecked paths:\n1. {path1}\n2. {path2}\n3. {path3}\n\nPlease ensure the guide file is in the correct location."; }
            } catch (Exception ex) {
                this.rtbGuideViewer.Text = $"Error loading scripting guide '{guideFilePath}': {ex.Message}";
            }
        }

        private void PopulateExampleScripts()
        {
            this.tvExamples.BeginUpdate();
            this.tvExamples.Nodes.Clear();
            foreach (var scriptTuple in ExampleScripts)
            {
                TreeNode node = new TreeNode(scriptTuple.Item1);
                node.Tag = scriptTuple.Item2;
                this.tvExamples.Nodes.Add(node);
            }
            this.tvExamples.EndUpdate();
            if (this.tvExamples.Nodes.Count > 0)
            {
                this.tvExamples.SelectedNode = this.tvExamples.Nodes[0];
            }
            else
            {
                this.fctbExampleViewer.Text = "// No examples loaded or found in temp_scripts.";
            }
        }

        private void tvExamples_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is string scriptCode)
            {
                this.fctbExampleViewer.Text = scriptCode;
                this.fctbExampleViewer.ClearUndo();
            }
            else
            {
                this.fctbExampleViewer.Text = "// Please select an example from the list.";
            }
        }

        private void btnCopyScript_Click(object sender, EventArgs e)
        {
            string scriptToCopy = this.fctbExampleViewer.Text;
            if (!string.IsNullOrEmpty(scriptToCopy) &&
                scriptToCopy != "// Please select an example from the list." &&
                scriptToCopy != "// No examples loaded." &&
                !scriptToCopy.StartsWith("// Error: Could not load") &&
                !scriptToCopy.StartsWith("// Error loading"))
            {
                try
                {
                    Clipboard.SetText(scriptToCopy);
                    Console.WriteLine("HelpForm: Example script copied to clipboard.");
                    // Optional: Could update a status label on HelpForm itself temporarily.
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"HelpForm: Error copying script to clipboard: {ex.Message}");
                    MessageBox.Show(this, "Could not copy script to clipboard. See application logs for details.", "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Console.WriteLine("HelpForm: No valid script content in viewer to copy.");
                // Optional: Feedback if there's nothing valid to copy
                // MessageBox.Show(this, "No script content selected or available to copy.", "Copy Script", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
