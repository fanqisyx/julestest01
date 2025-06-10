using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsUI
{
    public partial class ScriptSettingsForm : Form
    {
        private Label lblNamespaces;
        private TextBox txtNamespaces;
        private Label lblAssemblyRefs;
        private TextBox txtAssemblyRefs;
        private Button btnOK;
        private Button btnCancel;

        public List<string> AdditionalNamespaces { get; private set; }
        public List<string> AdditionalAssemblyReferences { get; private set; }

        public ScriptSettingsForm(List<string> currentNamespaces, List<string> currentAssemblyRefs)
        {
            InitializeComponentManual(); // Call to setup controls
            this.Text = "Script Settings";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(380, 280);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;

            // Initialize properties (new List to avoid modifying original list if Cancel is hit)
            AdditionalNamespaces = new List<string>(currentNamespaces ?? new List<string>());
            AdditionalAssemblyReferences = new List<string>(currentAssemblyRefs ?? new List<string>());

            // Populate TextBoxes
            txtNamespaces.Lines = AdditionalNamespaces.ToArray();
            txtAssemblyRefs.Lines = AdditionalAssemblyReferences.ToArray();

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void InitializeComponentManual()
        {
            this.SuspendLayout();

            // lblNamespaces
            this.lblNamespaces = new Label();
            this.lblNamespaces.AutoSize = true;
            this.lblNamespaces.Location = new System.Drawing.Point(12, 15);
            this.lblNamespaces.Name = "lblNamespaces";
            this.lblNamespaces.Text = "Additional Using Namespaces (one per line):";

            // txtNamespaces
            this.txtNamespaces = new TextBox();
            this.txtNamespaces.Location = new System.Drawing.Point(15, 35);
            this.txtNamespaces.Multiline = true;
            this.txtNamespaces.Name = "txtNamespaces";
            this.txtNamespaces.ScrollBars = ScrollBars.Vertical;
            this.txtNamespaces.Size = new System.Drawing.Size(350, 80);
            this.txtNamespaces.TabIndex = 0;
            this.txtNamespaces.AcceptsReturn = true;
            this.txtNamespaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));

            // lblAssemblyRefs
            this.lblAssemblyRefs = new Label();
            this.lblAssemblyRefs.AutoSize = true;
            this.lblAssemblyRefs.Location = new System.Drawing.Point(12, 130);
            this.lblAssemblyRefs.Name = "lblAssemblyRefs";
            this.lblAssemblyRefs.Text = "Additional Assembly References (one per line; full path or GAC name):";

            // txtAssemblyRefs
            this.txtAssemblyRefs = new TextBox();
            this.txtAssemblyRefs.Location = new System.Drawing.Point(15, 150);
            this.txtAssemblyRefs.Multiline = true;
            this.txtAssemblyRefs.Name = "txtAssemblyRefs";
            this.txtAssemblyRefs.ScrollBars = ScrollBars.Vertical;
            this.txtAssemblyRefs.Size = new System.Drawing.Size(350, 80);
            this.txtAssemblyRefs.TabIndex = 1;
            this.txtAssemblyRefs.AcceptsReturn = true;
            this.txtAssemblyRefs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));

            // btnOK
            this.btnOK = new Button();
            this.btnOK.Location = new System.Drawing.Point(210, 245);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.DialogResult = DialogResult.OK; // This will close the form with OK
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            // btnCancel
            this.btnCancel = new Button();
            this.btnCancel.Location = new System.Drawing.Point(290, 245);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel; // This will close the form with Cancel
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));

            // Add Controls to Form
            this.Controls.Add(this.lblNamespaces);
            this.Controls.Add(this.txtNamespaces);
            this.Controls.Add(this.lblAssemblyRefs);
            this.Controls.Add(this.txtAssemblyRefs);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.AdditionalNamespaces = this.txtNamespaces.Lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();
            this.AdditionalAssemblyReferences = this.txtAssemblyRefs.Lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();

            // DialogResult is already set to OK for this button, so form will close.
        }

        // In case a designer is ever used, it would generate this.
        // For manual setup, it's not strictly needed if not called.
        // private void InitializeComponent() {}
    }
}
