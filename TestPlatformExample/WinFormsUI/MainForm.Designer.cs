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
            this.SuspendLayout();
            //
            // lstLog
            //
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 15;
            this.lstLog.Location = new System.Drawing.Point(12, 50);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(760, 379);
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
            this.btnLoadPlugin.Text = "Load Sample Plugin";
            this.btnLoadPlugin.UseVisualStyleBackColor = true;
            this.btnLoadPlugin.Click += new System.EventHandler(this.btnLoadPlugin_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 441);
            this.Controls.Add(this.btnLoadPlugin);
            this.Controls.Add(this.btnRunTests);
            this.Controls.Add(this.lstLog);
            this.Name = "MainForm";
            this.Text = "Simple Test Platform Example";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.Button btnLoadPlugin;
    }
}
