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
            var plugins = _pluginManager.GetPlugins();
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    ListViewItem item = new ListViewItem(plugin.Name);
                    item.SubItems.Add(plugin.Description);
                    item.Tag = plugin;
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
                if (selectedItem.Tag is IPlugin plugin)
                {
                    StringBuilder details = new StringBuilder();
                    details.AppendLine($"Name: {plugin.Name}");
                    details.AppendLine($"Description: {plugin.Description}");
                    details.AppendLine($"Type: {plugin.GetType().FullName}");
                    details.AppendLine($"Assembly Location: {plugin.GetType().Assembly.Location}");
                    details.AppendLine();

                    if (plugin is IScriptablePlugin scriptablePlugin)
                    {
                        details.AppendLine("Scriptable (IScriptablePlugin): Yes");
                        try
                        {
                            string[] commands = scriptablePlugin.GetAvailableScriptCommands();
                            if (commands != null && commands.Length > 0)
                            {
                                details.AppendLine("Available Script Commands:");
                                foreach (string cmd in commands)
                                {
                                    details.AppendLine($"  - {cmd}");
                                }
                            }
                            else
                            {
                                details.AppendLine("Available Script Commands: None declared by plugin.");
                            }
                        }
                        catch (Exception ex)
                        {
                            details.AppendLine($"Available Script Commands: Error retrieving commands ({ex.GetType().Name}: {ex.Message}).");
                        }
                    }
                    else
                    {
                        details.AppendLine("Scriptable (IScriptablePlugin): No");
                    }

                    rtbPluginDetails.Text = details.ToString();
                }
                else
                {
                    rtbPluginDetails.Text = "Error: Selected item does not contain valid plugin data.";
                }
            }
            else
            {
                rtbPluginDetails.Text = "Select a plugin from the list to see its details.";
            }
        }
    }
}
