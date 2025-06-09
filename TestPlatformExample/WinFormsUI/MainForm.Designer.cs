// It's generally recommended to add specific using statements here if types from those namespaces are used directly in the designer code.
// However, some prefer to keep this file as clean as possible and use fully qualified names or ensure usings are in the main .cs file.
// For FastColoredTextBoxNS.Language, it's cleaner to have the using statement.
using FastColoredTextBoxNS; // Added for Language.CSharp

namespace WinFormsUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container(); // Often added by designer for FCTB
            this.lstLog = new System.Windows.Forms.ListBox();
            this.btnRunTests = new System.Windows.Forms.Button();
            this.btnLoadPlugin = new System.Windows.Forms.Button();
            // this.txtScriptInput = new System.Windows.Forms.TextBox(); // Removed
            this.btnRunScript = new System.Windows.Forms.Button();
            this.fctbScriptInput = new FastColoredTextBox(); // Added: Assuming class name is FastColoredTextBox
            this.SuspendLayout();
            //
            // lstLog
            //
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 15;
            this.lstLog.Location = new System.Drawing.Point(12, 50);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(760, 270);
            this.lstLog.TabIndex = 0;
            //
            // btnRunTests
            //
            this.btnRunTests.Location = new System.Drawing.Point(130, 12);
            this.btnRunTests.Name = "btnRunTests";
            this.btnRunTests.Size = new System.Drawing.Size(112, 32);
            this.btnRunTests.TabIndex = 1;
            this.btnRunTests.Text = "Run Plugin Tests";
            this.btnRunTests.UseVisualStyleBackColor = true;
            this.btnRunTests.Click += new System.EventHandler(this.btnRunTests_Click);
            //
            // btnLoadPlugin
            //
            this.btnLoadPlugin.Location = new System.Drawing.Point(12, 12);
            this.btnLoadPlugin.Name = "btnLoadPlugin";
            this.btnLoadPlugin.Size = new System.Drawing.Size(112, 32);
            this.btnLoadPlugin.TabIndex = 2;
            this.btnLoadPlugin.Text = "Load Plugins";
            this.btnLoadPlugin.UseVisualStyleBackColor = true;
            this.btnLoadPlugin.Click += new System.EventHandler(this.btnLoadPlugin_Click);
            //
            // btnRunScript
            //
            this.btnRunScript.Location = new System.Drawing.Point(248, 12);
            this.btnRunScript.Name = "btnRunScript";
            this.btnRunScript.Size = new System.Drawing.Size(112, 32);
            this.btnRunScript.TabIndex = 4; // TabIndex 3 will be for fctbScriptInput
            this.btnRunScript.Text = "Run Script";
            this.btnRunScript.UseVisualStyleBackColor = true;
            this.btnRunScript.Click += new System.EventHandler(this.btnRunScript_Click);
            //
            // fctbScriptInput
            //
            this.fctbScriptInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fctbScriptInput.Location = new System.Drawing.Point(12, 325);
            this.fctbScriptInput.Name = "fctbScriptInput";
            this.fctbScriptInput.Size = new System.Drawing.Size(760, 100);
            this.fctbScriptInput.TabIndex = 3; // Correct TabIndex
            this.fctbScriptInput.Language = FastColoredTextBoxNS.Language.CSharp; // Using fully qualified for safety here, or use 'using' at top
            this.fctbScriptInput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.fctbScriptInput.LineNumberColor = System.Drawing.Color.Teal;
            this.fctbScriptInput.ShowLineNumbers = true;
            this.fctbScriptInput.IndentBackColor = System.Drawing.Color.WhiteSmoke;
            this.fctbScriptInput.ServiceLinesColor = System.Drawing.Color.Silver; // Example property
            this.fctbScriptInput.CaretColor = System.Drawing.Color.Red; // Example property
            this.fctbScriptInput.ShowFoldingLines = true; // Example property
            this.fctbScriptInput.AutoCompleteBrackets = true; // Example property
            this.fctbScriptInput.AutoScrollMinSize = new System.Drawing.Size(2, 14); // Default, adjust if needed
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 437);
            this.Controls.Add(this.fctbScriptInput);   // Added fctbScriptInput
            // this.Controls.Add(this.txtScriptInput); // Removed txtScriptInput
            this.Controls.Add(this.btnRunScript);
            this.Controls.Add(this.btnLoadPlugin);
            this.Controls.Add(this.btnRunTests);
            this.Controls.Add(this.lstLog);
            this.Name = "MainForm";
            this.Text = "Simple Test Platform Example - Scripting";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.Button btnLoadPlugin;
        // private System.Windows.Forms.TextBox txtScriptInput; // Removed
        private System.Windows.Forms.Button btnRunScript;
        private FastColoredTextBox fctbScriptInput; // Added - assuming class name is FastColoredTextBox
    }
}
