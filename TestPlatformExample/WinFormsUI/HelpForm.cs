using System;
using System.Drawing;
using System.Windows.Forms;
using FastColoredTextBoxNS; // For fctbExampleViewer
using System.IO; // For File and Path operations

namespace WinFormsUI
{
    public partial class HelpForm : Form
    {
        private SplitContainer splitContainerMain;
        private RichTextBox rtbGuideViewer;

        private SplitContainer splitContainerExamples;
        private TreeView tvExamples;
        private FastColoredTextBox fctbExampleViewer;
        private Button btnCopyScript;

        public HelpForm()
        {
            InitializeComponentManual();
            this.Text = "Scripting Help & Examples";
            this.Size = new System.Drawing.Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            LoadGuideContent(); // Call to load the guide
        }

        private void InitializeComponentManual()
        {
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
            this.splitContainerMain.SplitterDistance = 300; // Adjusted for potentially more guide text
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

            // Path assumptions for finding the guide:
            // 1. Same directory as executable (common for deployed files)
            string path1 = Path.Combine(Application.StartupPath, guideFileName);
            // 2. In the project root of TestPlatformExample (common when running from IDE, StartupPath is .../WinFormsUI/bin/Debug/net8.0)
            //    So, StartupPath -> WinFormsUI/bin/Debug/net8.0 -> ../../../ -> TestPlatformExample/
            string path2 = Path.Combine(Application.StartupPath, "..", "..", "..", guideFileName);
            // 3. One level up from executable (less common, but for shallow build structures)
            string path3 = Path.Combine(Application.StartupPath, "..", guideFileName);


            if (File.Exists(path1))
            {
                guideFilePath = path1;
                fileFound = true;
            }
            else if (File.Exists(path2))
            {
                guideFilePath = path2;
                fileFound = true;
            }
            else if (File.Exists(path3))
            {
                 guideFilePath = path3;
                 fileFound = true;
            }

            try
            {
                if (fileFound)
                {
                    this.rtbGuideViewer.Text = File.ReadAllText(guideFilePath);
                    // For basic Markdown display in RichTextBox, this is plain text.
                    // For formatted Markdown, a library or more complex parsing would be needed.
                    // This subtask implies loading the raw Markdown text.
                }
                else
                {
                    this.rtbGuideViewer.Text = $"Error: Could not find '{guideFileName}'.\nChecked paths:\n1. {path1}\n2. {path2}\n3. {path3}\n\nPlease ensure the guide file is in the correct location.";
                }
            }
            catch (Exception ex)
            {
                this.rtbGuideViewer.Text = $"Error loading scripting guide '{guideFilePath}': {ex.Message}";
            }
        }
    }
}
