using System;
using System.Drawing;
using System.Text; // For StringBuilder
using System.Windows.Forms;
using CorePlatform; // For PluginManager, IPlugin, IScriptablePlugin
using System.Linq; // For .Any() if used, or general LINQ goodness
using System.Collections.Generic; // For List (used in PopulatePluginList implicitly)

namespace WinFormsUI
{
    public partial class PluginInfoForm : Form
    {
        private SplitContainer splitContainerMain;
        private ListView lvPlugins;
        private RichTextBox rtbPluginDetails;
        private Button btnClose;

        private PluginManager _pluginManager;
        private System.ComponentModel.IContainer components = null;

        public PluginInfoForm(PluginManager pluginManager)
        {
            _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));

            InitializeComponentManual();
            this.Text = "Loaded Plugin Information";
            this.Size = new System.Drawing.Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            PopulatePluginList();
            this.Shown += (s, e) => {
                if (lvPlugins.Items.Count > 0)
                {
                    lvPlugins.Items[0].Focused = true;
                    lvPlugins.Items[0].Selected = true;
                }
            };
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

            this.splitContainerMain = new SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();

            this.splitContainerMain.Dock = DockStyle.Fill;
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.SplitterDistance = 250;
            this.splitContainerMain.Orientation = Orientation.Vertical;

            this.lvPlugins = new ListView();
            this.lvPlugins.Dock = DockStyle.Fill;
            this.lvPlugins.FullRowSelect = true;
            this.lvPlugins.HideSelection = false;
            this.lvPlugins.MultiSelect = false;
            this.lvPlugins.Name = "lvPlugins";
            this.lvPlugins.View = View.Details;
            this.lvPlugins.Columns.Add("Name", 120, HorizontalAlignment.Left);
            this.lvPlugins.Columns.Add("Description", 200, HorizontalAlignment.Left);
            this.lvPlugins.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.lvPlugins.SelectedIndexChanged += new EventHandler(this.lvPlugins_SelectedIndexChanged);
            this.lvPlugins.Font = new Font("Segoe UI", 9F);

            this.rtbPluginDetails = new RichTextBox();
            this.rtbPluginDetails.Dock = DockStyle.Fill;
            this.rtbPluginDetails.Name = "rtbPluginDetails";
            this.rtbPluginDetails.ReadOnly = true;
            this.rtbPluginDetails.Font = new Font("Consolas", 9.75F);
            this.rtbPluginDetails.BorderStyle = BorderStyle.FixedSingle;
            this.rtbPluginDetails.WordWrap = true;

            this.btnClose = new Button();
            this.btnClose.Dock = DockStyle.Bottom;
            this.btnClose.Name = "btnClose";
            this.btnClose.Text = "Close";
            this.btnClose.Size = new Size(75, 28);
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += (s, e) => this.Close();
            this.btnClose.DialogResult = DialogResult.Cancel;

            this.splitContainerMain.Panel1.Controls.Add(this.lvPlugins);
            this.splitContainerMain.Panel2.Controls.Add(this.rtbPluginDetails);
            this.splitContainerMain.Panel2.Controls.Add(this.btnClose);

            this.Controls.Add(this.splitContainerMain);
            this.CancelButton = this.btnClose;

            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PopulatePluginList()
        {
            lvPlugins.Items.Clear();
            var factories = _pluginManager.GetPluginFactories(); // Changed to GetPluginFactories
            if (factories != null)
            {
                foreach (var factory in factories) // Iterate through factories
                {
                    ListViewItem item = new ListViewItem(factory.TypeName); // Use TypeName
                    item.SubItems.Add(factory.TypeDescription); // Use TypeDescription
                    item.Tag = factory; // Tag is now IPluginFactory
                    lvPlugins.Items.Add(item);
                }
            }

            if (lvPlugins.Items.Count > 0) {
                lvPlugins.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                if (lvPlugins.Columns.Count > 0 && lvPlugins.Columns[0].Width < 100) lvPlugins.Columns[0].Width = 100;
                if (lvPlugins.Columns.Count > 1 && lvPlugins.Columns[1].Width < 150) lvPlugins.Columns[1].Width = 150;
            } else {
                 if (lvPlugins.Columns.Count == 0) {
                    lvPlugins.Columns.Add("Name", 120, HorizontalAlignment.Left);
                    lvPlugins.Columns.Add("Description", 200, HorizontalAlignment.Left);
                 } else {
                    lvPlugins.Columns[0].Width = 120;
                    lvPlugins.Columns[1].Width = 200;
                 }
            }
        }

        private void lvPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvPlugins.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = lvPlugins.SelectedItems[0];
                if (selectedItem.Tag is IPluginFactory factory) // Changed to IPluginFactory
                {
                    StringBuilder details = new StringBuilder();
                    details.AppendLine($"Factory Type Name: {factory.TypeName}"); // Use factory.TypeName
                    details.AppendLine($"Factory Type Description: {factory.TypeDescription}"); // Use factory.TypeDescription
                    details.AppendLine($"Factory ID: {factory.FactoryId}");
                    details.AppendLine($"Factory Implementation Type: {factory.GetType().FullName}");
                    details.AppendLine($"Assembly Location: {factory.GetType().Assembly.Location}");
                    details.AppendLine();

                    if (factory is IScriptablePluginFactory scriptableFactory) // Changed to IScriptablePluginFactory
                    {
                        details.AppendLine("Scriptable Factory (IScriptablePluginFactory): Yes");
                        // IScriptablePluginFactory does not define GetAvailableScriptCommands.
                        // This information is instance-specific.
                        // The section for listing commands has been removed for the factory view.
                        // If needed, one might display generic command *types* the factory can produce,
                        // but not specific commands for non-existent instances.
                    }
                    else
                    {
                        details.AppendLine("Scriptable Factory (IScriptablePluginFactory): No");
                    }

                    rtbPluginDetails.Text = details.ToString();
                }
                else
                {
                    rtbPluginDetails.Text = "Error: Selected item does not contain valid factory data.";
                }
            }
            else
            {
                rtbPluginDetails.Text = "Select a plugin factory from the list to see its details.";
            }
        }
    }
}
