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

        private static readonly Tuple<string, string>[] ExampleScripts = new Tuple<string, string>[]
        {
            Tuple.Create("1. Basic Logging",
                "Host.Log(\"Hello from a C# script!\");\n" +
                "Host.Log(\"This is another log entry.\");\n" +
                "int a = 10;\n" +
                "int b = 20;\n" +
                "Host.Log($\"The sum of {a} and {b} is {a + b}.\");\n" +
                "// Script return value (last expression) will be a+b\n" +
                "a+b;"),

            Tuple.Create("2. List Loaded Plugins",
                "Host.Log(\"Attempting to list loaded plugins...\");\n" +
                "string[] pluginNames = Host.ListPluginNames();\n" +
                "if (pluginNames.Length == 0) {\n" +
                "    Host.Log(\"No plugins are currently loaded.\");\n" +
                "} else {\n" +
                "    Host.Log(\"Currently loaded plugins are:\");\n" +
                "    foreach (string name in pluginNames) {\n" +
                "        Host.Log($\"- {name}\");\n" +
                "    }\n" +
                "}"),

            Tuple.Create("3. Execute Command on SamplePlugin",
                "// Ensure 'Sample Test Plugin' is loaded first via MainForm's 'Load Plugins' button.\n" +
                "string targetPlugin = \"Sample Test Plugin\"; // Plugin's Name property\n\n" +
                "Host.Log($\"Attempting to execute 'GetStatus' on '{targetPlugin}'...\");\n" +
                "string? statusResult = Host.ExecutePluginCommand(targetPlugin, \"GetStatus\", null);\n" +
                "Host.Log($\"'{targetPlugin}' GetStatus result: {(statusResult ?? \"null\")}\");\n\n" +
                "Host.Log($\"Attempting to execute 'Echo' on '{targetPlugin}'...\");\n" +
                "string? echoResult = Host.ExecutePluginCommand(targetPlugin, \"Echo\", \"Hello from Script Example 3!\"); \n" +
                "Host.Log($\"'{targetPlugin}' Echo result: {(echoResult ?? \"null\")}\");\n\n" +
                "Host.Log($\"Attempting to execute 'Add' on '{targetPlugin}'...\");\n" +
                "string? addResult = Host.ExecutePluginCommand(targetPlugin, \"Add\", \"123,456\"); \n" +
                "Host.Log($\"'{targetPlugin}' Add result: {(addResult ?? \"null\")}\");"),

            Tuple.Create("4. Handle Plugin Not Found",
                "string nonExistentPlugin = \"FakePlugin123\";\n" +
                "Host.Log($\"Attempting to execute a command on a non-existent plugin: '{nonExistentPlugin}'...\");\n" +
                "string? result = Host.ExecutePluginCommand(nonExistentPlugin, \"AnyCommand\", \"anyparams\");\n" +
                "Host.Log($\"Result for '{nonExistentPlugin}': {(result ?? \"null\")}\"); \n" +
                "// Expected: Result will contain an error message."),

            Tuple.Create("5. Use MessageBox (WinForms)",
                "Host.Log(\"Attempting to show a MessageBox.\");\n" +
                "// System.Windows.Forms should be available due to ScriptEngine defaults if host is WinForms.\n" +
                "DialogResult dr = MessageBox.Show(\"Hello from script!\", \"Script Info\", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);\n" +
                "Host.Log(\"MessageBox result: \" + dr.ToString());\n" +
                "if (dr == DialogResult.OK) {\n" +
                "    Host.Log(\"User clicked OK.\");\n" +
                "} else {\n" +
                "    Host.Log(\"User clicked Cancel or closed the dialog.\");\n" +
                "}")
        };

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
            this.btnCopyScript.Click += new System.EventHandler(this.btnCopyScript_Click); // Wired up

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
                this.fctbExampleViewer.Text = "// No examples loaded.";
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
            if (!string.IsNullOrEmpty(scriptToCopy))
            {
                try
                {
                    Clipboard.SetText(scriptToCopy);
                    // Optionally, provide feedback to the user. For example:
                    // MessageBox.Show(this, "Script copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine("HelpForm: Script copied to clipboard."); // For debugging
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"HelpForm: Error copying script to clipboard: {ex.Message}");
                    // Optionally, inform the user of the error:
                    // MessageBox.Show(this, "Could not copy script to clipboard.\n" + ex.Message, "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
