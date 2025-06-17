using System;
using System.Windows.Forms; // For UserControl, Label, TextBox, DockStyle, Padding etc.
using System.Drawing;     // For Size, Point

namespace SamplePlugin
{
    public partial class SamplePluginSettingsUI : UserControl
    {
        private Label lblInstanceName;
        private TextBox txtInstanceName;

        public SamplePluginSettingsUI(SamplePluginInstanceConfig? initialConfig)
        {
            InitializeComponent(); // Call the method that sets up controls

            if (initialConfig != null)
            {
                txtInstanceName.Text = initialConfig.InstanceDisplayName;
            }
            else
            {
                txtInstanceName.Text = "New Sample Instance"; // Default for new instances
            }
        }

        // Manual equivalent of InitializeComponent()
        private void InitializeComponent()
        {
            this.lblInstanceName = new Label();
            this.txtInstanceName = new TextBox();
            this.SuspendLayout();

            // lblInstanceName
            this.lblInstanceName.AutoSize = true;
            this.lblInstanceName.Location = new Point(10, 13);
            this.lblInstanceName.Name = "lblInstanceName";
            this.lblInstanceName.Size = new Size(100, 15); // Approx size, AutoSize is true
            this.lblInstanceName.TabIndex = 0;
            this.lblInstanceName.Text = "Instance Name:";

            // txtInstanceName
            this.txtInstanceName.Location = new Point(120, 10);
            this.txtInstanceName.Name = "txtInstanceName";
            this.txtInstanceName.Size = new Size(200, 23);
            this.txtInstanceName.TabIndex = 1;
            this.txtInstanceName.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);


            // SamplePluginSettingsUI
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.txtInstanceName);
            this.Controls.Add(this.lblInstanceName);
            this.Name = "SamplePluginSettingsUI";
            this.Padding = new Padding(5);
            this.Size = new Size(330, 50); // Small UserControl
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public SamplePluginInstanceConfig GetSettings()
        {
            return new SamplePluginInstanceConfig
            {
                InstanceDisplayName = this.txtInstanceName.Text.Trim()
            };
        }

        // Optional: A method to load settings if needed externally after construction,
        // though constructor injection is generally preferred.
        public void LoadSettings(SamplePluginInstanceConfig? config)
        {
            if (config != null)
            {
                txtInstanceName.Text = config.InstanceDisplayName;
            }
            else
            {
                txtInstanceName.Text = string.Empty;
            }
        }
    }
}
