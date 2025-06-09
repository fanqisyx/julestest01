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
            this.lstLog = new System.Windows.Forms.ListBox();
            this.btnRunTests = new System.Windows.Forms.Button();
            this.btnLoadPlugin = new System.Windows.Forms.Button();
            this.txtScriptInput = new System.Windows.Forms.TextBox(); // Added
            this.btnRunScript = new System.Windows.Forms.Button();   // Added
            this.SuspendLayout();
            //
            // lstLog
            //
            // Original Anchor: Top, Bottom, Left, Right
            // New Anchor: Top, Left, Right (to not overlap with txtScriptInput when resizing)
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 15;
            this.lstLog.Location = new System.Drawing.Point(12, 50);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(760, 270); // Adjusted height
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
            this.btnLoadPlugin.Size = new System.Drawing.Size(112, 32); // Text will be updated in MainForm.cs if needed
            this.btnLoadPlugin.TabIndex = 2;
            this.btnLoadPlugin.Text = "Load Plugins"; // Updated text
            this.btnLoadPlugin.UseVisualStyleBackColor = true;
            this.btnLoadPlugin.Click += new System.EventHandler(this.btnLoadPlugin_Click);
            //
            // txtScriptInput
            //
            this.txtScriptInput.AcceptsReturn = true;
            this.txtScriptInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScriptInput.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtScriptInput.Location = new System.Drawing.Point(12, 325); // Y = 50 (lstLog.Top) + 270 (lstLog.Height) + 5 (spacing)
            this.txtScriptInput.Multiline = true;
            this.txtScriptInput.Name = "txtScriptInput";
            this.txtScriptInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtScriptInput.Size = new System.Drawing.Size(760, 100);
            this.txtScriptInput.TabIndex = 3;
            this.txtScriptInput.WordWrap = false;
            //
            // btnRunScript
            //
            this.btnRunScript.Location = new System.Drawing.Point(248, 12); // Positioned next to btnRunTests
            this.btnRunScript.Name = "btnRunScript";
            this.btnRunScript.Size = new System.Drawing.Size(112, 32);
            this.btnRunScript.TabIndex = 4;
            this.btnRunScript.Text = "Run Script";
            this.btnRunScript.UseVisualStyleBackColor = true;
            this.btnRunScript.Click += new System.EventHandler(this.btnRunScript_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 437); // Adjusted height: 325 (txtScriptInput.Top) + 100 (txtScriptInput.Height) + 12 (padding)
            this.Controls.Add(this.txtScriptInput);    // Added
            this.Controls.Add(this.btnRunScript);      // Added
            this.Controls.Add(this.btnLoadPlugin);
            this.Controls.Add(this.btnRunTests);
            this.Controls.Add(this.lstLog);
            this.Name = "MainForm";
            this.Text = "Simple Test Platform Example - Scripting"; // Updated title
            this.ResumeLayout(false);
            this.PerformLayout(); // Added this line, often needed when adding controls manually to ensure layout calculations

        }

        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.Button btnLoadPlugin;
        private System.Windows.Forms.TextBox txtScriptInput; // Added
        private System.Windows.Forms.Button btnRunScript;   // Added
    }
}
