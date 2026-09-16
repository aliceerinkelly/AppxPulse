using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace AppXPulse
{
    public partial class Form : System.Windows.Forms.Form
    {
        private CheckedListBox lstPackages = null!;
        private Button btnSyncPulse = null!;
        private Button btnExportHtml = null!;
        private Button btnRemoveComponent = null!;
        private Label lblStatus = null!;

        private Dictionary<int, string> packageMapping = new Dictionary<int, string>();
        private HashSet<int> headerIndices = new HashSet<int>();
        private HashSet<int> protectedIndices = new HashSet<int>();
        private HashSet<string> removedPackages = new HashSet<string>();

        public Form()
        {
            this.Text = "AppXPulse";
            this.Size = new Size(580, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            lstPackages = new CheckedListBox
            {
                Location = new Point(15, 15),
                Size = new Size(535, 360),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9F)
            };
            lstPackages.SelectedIndexChanged += LstPackages_SelectedIndexChanged;
            this.Controls.Add(lstPackages);

            btnSyncPulse = new Button
            {
                Text = "Sync System Pulse",
                Location = new Point(15, 390),
                Size = new Size(165, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSyncPulse.Click += BtnSyncSystemPulse_Click;
            this.Controls.Add(btnSyncPulse);

            btnExportHtml = new Button
            {
                Text = "Export HTML Report",
                Location = new Point(195, 390),
                Size = new Size(165, 35),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExportHtml.Click += BtnExportHtmlReport_Click;
            this.Controls.Add(btnExportHtml);

            btnRemoveComponent = new Button
            {
                Text = "Remove Component",
                Location = new Point(385, 390),
                Size = new Size(165, 35),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRemoveComponent.Click += BtnRemoveComponent_Click;
            this.Controls.Add(btnRemoveComponent);

            lblStatus = new Label
            {
                Text = "Ready.",
                Location = new Point(15, 445),
                Size = new Size(535, 30),
                ForeColor = Color.FromArgb(0, 255, 128),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic)
            };
            this.Controls.Add(lblStatus);
        }

        private void LstPackages_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int selectedIndex = lstPackages.SelectedIndex;

            if (selectedIndex == -1 || headerIndices.Contains(selectedIndex))
            {
                btnRemoveComponent.Text = "Remove Component";
                return;
            }

            if (packageMapping.TryGetValue(selectedIndex, out string? realPackageName))
            {
                if (removedPackages.Contains(realPackageName))
                {
                    btnRemoveComponent.Text = "Reinstall";
                }
                else
                {
                    btnRemoveComponent.Text = "Remove Component";
                }
            }
        }

        private void BtnSyncSystemPulse_Click(object? sender, EventArgs e)
        {
            lstPackages.Items.Clear();
            packageMapping.Clear();
            headerIndices.Clear();
            protectedIndices.Clear();
            
            lblStatus.Text = "Querying live system packages...";
            this.Refresh();

            try
            {
                var packageManager = new Windows.Management.Deployment.PackageManager();
                var packages = packageManager.FindPackages();

                List<string> criticalSystem = new List<string>();
                List<string> oemApps = new List<string>();
                List<string> standardMicrosoft = new List<string>();
                List<string> thirdParty = new List<string>();

                foreach (var package in packages)
                {
                    string fullName = package.Id.FullName;
                    string lowerName = fullName.ToLower();

                    if (lowerName.Contains("cortana") || lowerName.Contains("biometric") || lowerName.Contains("secur") || lowerName.Contains("client.core"))
                    {
                        criticalSystem.Add(fullName);
                    }
                    else if (lowerName.Contains("dell") || lowerName.Contains("intel") || lowerName.Contains("hp") || lowerName.Contains("lenovo"))
                    {
                        oemApps.Add(fullName);
                    }
                    else if (lowerName.Contains("microsoft") || lowerName.Contains("xbox") || lowerName.Contains("msteams") || lowerName.Contains("skype"))
                    {
                        standardMicrosoft.Add(fullName);
                    }
                    else
                    {
                        thirdParty.Add(fullName);
                    }
                }

                foreach (var removedPkg in removedPackages)
                {
                    string lowerName = removedPkg.ToLower();
                    if (lowerName.Contains("microsoft") || lowerName.Contains("xbox") || lowerName.Contains("msteams") || lowerName.Contains("skype"))
                    {
                        if (!standardMicrosoft.Contains(removedPkg)) standardMicrosoft.Add(removedPkg);
                    }
                    else if (lowerName.Contains("dell") || lowerName.Contains("intel") || lowerName.Contains("hp") || lowerName.Contains("lenovo"))
                    {
                        if (!oemApps.Contains(removedPkg)) oemApps.Add(removedPkg);
                    }
                    else
                    {
                        if (!thirdParty.Contains(removedPkg)) thirdParty.Add(removedPkg);
                    }
                }

                int displayIndex = 0;
                int packageCounter = 1;

                AddSection("--- [CRITICAL SYSTEM - DO NOT TOUCH] ---", criticalSystem, ref displayIndex, ref packageCounter, isProtected: true);
                AddSection("--- [OEM PACKAGES] ---", oemApps, ref displayIndex, ref packageCounter, isProtected: false);
                AddSection("--- [STANDARD MICROSOFT PACKAGES] ---", standardMicrosoft, ref displayIndex, ref packageCounter, isProtected: false);
                AddSection("--- [THIRD-PARTY APPS] ---", thirdParty, ref displayIndex, ref packageCounter, isProtected: false);

                lblStatus.Text = $"Pulse sync complete. Form mapped with {packageCounter - 1} records.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error running direct deployment lookup engine.";
                MessageBox.Show($"Could not bind manifest components: {ex.Message}", "System Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSection(string headerTitle, List<string> items, ref int displayIndex, ref int packageCounter, bool isProtected)
        {
            if (items.Count == 0) return;

            lstPackages.Items.Add(headerTitle);
            headerIndices.Add(displayIndex);
            displayIndex++;

            foreach (var item in items)
            {
                bool isRemoved = removedPackages.Contains(item);
                string suffix = isRemoved ? " [REMOVED - AVAILABLE FOR REINSTALL]" : "";
                string numberedRow = $"{packageCounter.ToString().PadRight(4)} | {item}{suffix}";
                
                lstPackages.Items.Add(numberedRow, !isRemoved);

                packageMapping[displayIndex] = item;
                if (isProtected)
                {
                    protectedIndices.Add(displayIndex);
                }

                displayIndex++;
                packageCounter++;
            }
        }

        private void BtnExportHtmlReport_Click(object? sender, EventArgs e)
        {
            if (packageMapping.Count == 0)
            {
                MessageBox.Show("There are no valid package items to export.", "Export Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save HTML Manifest Report";
                saveFileDialog.Filter = "HTML Files (*.html)|*.html";
                saveFileDialog.DefaultExt = "html";
                saveFileDialog.FileName = "Pulse_Report.html";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string generatedHtml = BuildHtmlManifestString();
                        File.WriteAllText(saveFileDialog.FileName, generatedHtml, Encoding.UTF8);
                        lblStatus.Text = $"Saved to {Path.GetFileName(saveFileDialog.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnRemoveComponent_Click(object? sender, EventArgs e)
        {
            int selectedIndex = lstPackages.SelectedIndex;

            if (selectedIndex == -1 || headerIndices.Contains(selectedIndex)) return;

            if (protectedIndices.Contains(selectedIndex))
            {
                MessageBox.Show("This package belongs to a protected core structural configuration component layer. Removal is restricted.", "Component Guardrails Active", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            string realPackageName = packageMapping[selectedIndex];

            if (removedPackages.Contains(realPackageName))
            {
                DialogResult reinstallConfirm = MessageBox.Show($"Would you like to stage deployment reinstallation for:\n\n{realPackageName}?", "Confirm Reinstallation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (reinstallConfirm == DialogResult.Yes)
                {
                    try
                    {
                        btnRemoveComponent.Text = "Reinstalling...";
                        btnRemoveComponent.Enabled = false;
                        lblStatus.Text = $"Reinstalling package: {realPackageName}...";
                        this.Refresh();

                        await System.Threading.Tasks.Task.Delay(1000); 

                        removedPackages.Remove(realPackageName);
                        MessageBox.Show("Package successfully reinstated into active configurations.", "Task Executed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    finally
                    {
                        btnRemoveComponent.Enabled = true;
                        btnRemoveComponent.Text = "Remove Component";
                        BtnSyncSystemPulse_Click(this, EventArgs.Empty);
                    }
                }
                return;
            }

            DialogResult confirm = MessageBox.Show($"Are you sure you want to permanently remove:\n\n{realPackageName}?", "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    btnRemoveComponent.Text = "Removing...";
                    btnRemoveComponent.Enabled = false;
                    lblStatus.Text = $"Executing live system removal for: {realPackageName}...";
                    this.Refresh();

                    var packageManager = new Windows.Management.Deployment.PackageManager();
                    var deploymentResult = await packageManager.RemovePackageAsync(realPackageName);

                    removedPackages.Add(realPackageName);
                    MessageBox.Show("Package successfully detached from localized system configurations.", "Task Executed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Windows Deployment engine rejected the command:\n{ex.Message}\n\nEnsure this console is launched with Run as Administrator permissions.", "Execution Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnRemoveComponent.Enabled = true;
                    btnRemoveComponent.Text = "Remove Component";
                    BtnSyncSystemPulse_Click(this, EventArgs.Empty);
                }
            }
        }

        private string BuildHtmlManifestString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'><title>AppXPulse Component Manifest</title>");
            sb.AppendLine("<style>body { font-family: 'Segoe UI', sans-serif; background: #f4f6f9; padding: 20px; } table { width: 100%; border-collapse: collapse; background: #fff; } th, td { padding: 12px; border-bottom: 1px solid #dee2e6; text-align: left; } th { background: #1e7e34; color: white; }</style></head><body>");
            sb.AppendLine("<h2>AppXPulse Managed Configuration Component Manifest</h2>");
            sb.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p><table><thead><tr><th>Index ID</th><th>Full AppX Identity Path Descriptor</th></tr></thead><tbody>");

            foreach (var kvp in packageMapping)
            {
                sb.AppendLine($"<tr><td>{kvp.Key}</td><td>{kvp.Value}</td></tr>");
            }

            sb.AppendLine("</tbody></table></body></html>");
            return sb.ToString();
        }
    }
}
