using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Tools
{
    public partial class Form1 : Form
    {
        private const int BaseRightControlsStartX = 657;
        private const int PanelGap = 6;
        private const int AddButtonReservedWidth = 28;

        public Form1()
        {
            InitializeComponent();
            lista.loadPC();
            lista.loadListPC();
            lista.loadListBTN();
            lista.loadListCHK();
            caricaBtn();
            caricaCheck();
            function.SetLogWriter(AppendLog);
            configureCredentialMenu();
            setupPanelTitles();
            addCategoryAddButtons();
            wireMachineContextMenus();
            groupBox1.Visible = false;
            relayoutPanelsAndResizeForm();
            textDest.Text = @"\sms-xxx\bin\  (es.)";
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        Liste lista = new Liste();
        Funzioni function = new Funzioni();
        static int qualeRadio = 0;

        public static int getQualeradio { get => qualeRadio; }

        private void caricaBtn()
        {

            foreach (Button btn in lista.getCmpButton)
            {
                panelCMP.Controls.Add(btn);
            }
            foreach (Button btn in lista.getTrdButton)
            {
                panelTRD.Controls.Add(btn);
            }
            foreach (Button btn in lista.getDokButton)
            {
                panelDOK.Controls.Add(btn);
            }
            foreach (Button btn in lista.getSrvButton)
            {
                panelSRV.Controls.Add(btn);
            }
            foreach (Button btn in lista.getGwButton)
            {
                panelGW.Controls.Add(btn);
            }
            foreach (Button btn in lista.getMfcButton)
            {
                panelMfc.Controls.Add(btn);
            }

        }
        private void caricaCheck()
        {


            foreach (CheckBox check in lista.getCmpCheck)
            {
                panelCMP.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getTrdCheck)
            {
                panelTRD.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getDokCheck)
            {
                panelDOK.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getSrvCheck)
            {
                panelSRV.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getGwCheck)
            {
                panelGW.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getMfcCheck)
            {
                panelMfc.Controls.Add(check);
            }


        }

       
        //PROVA ping ASYNC iniziale al momento non è asincrono
        private async Task selTutto() {
            foreach (CheckBox check in lista.getlistaCheck) check.Checked = true;
        }

        //NUOVO SX SEL TUTTO, DX DESEL. TUTTO
        private void panelCMP_MouseClick(object sender, MouseEventArgs e)
        {
   
                foreach (CheckBox check in lista.getCmpCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
        }

        private void panelTRD_MouseClick(object sender, MouseEventArgs e)
        {
                    foreach (CheckBox check in lista.getTrdCheck)
                    {
                        if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        {
                            check.Checked = false;
                        }
                        else
                        {
                            check.Checked = true;
                        }
                    }
        }

        private void panelSRV_MouseClick(object sender, MouseEventArgs e)
        {
                    foreach (CheckBox check in lista.getSrvCheck)
                    {
                        if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        {
                            check.Checked = false;
                        }
                        else
                        {
                            check.Checked = true;
                        }
                    }
            }

        private void panelGW_MouseClick(object sender, MouseEventArgs e)
        {
                foreach (CheckBox check in lista.getGwCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
            }

        private void panelDOK_MouseClick(object sender, MouseEventArgs e)
        {
                foreach (CheckBox check in lista.getDokCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
        }
        private void panelMFC_MouseClick(object sender, MouseEventArgs e)
        {
                foreach (CheckBox check in lista.getMfcCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
        }

        private async void ping_MouseClick(object sender, MouseEventArgs e)
        {
            refresh();
            await function.evento(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "ping");
        }

        private void refresh()
        {
            foreach (Button btn in lista.getlistaButton)
            {
                btn.Text = btn.Name.ToUpper();
                btn.BackColor = Color.LightGray;
            }
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private async void Riavvio_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.label1.Text = "Sicuro di voler riavviare le macchine selezionate?";
            form2.ShowDialog();
           
            if (form2.getScelta == 1)
            {
                form2.Close();
                await function.evento(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "riavvio");
            }
            form2.Close();
        }

        private async void off_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.label1.Text= "Sicuro di voler spegnere le macchine selezionate?";   
            form2.ShowDialog();
            if (form2.getScelta == 1)
            {
                form2.Close();
                await function.evento(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "off");
            }
            form2.Close();
        }

        private async void Kill_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.label1.Text = "Vuoi Killare i proc. sulle macchine selezionate?";
            form2.ShowDialog();
            if (form2.getScelta == 1)
            {
                form2.Close();
                await function.processKill(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck);
            }
            form2.Close();
        }

        private void Changeradio(object sender, EventArgs e)
        {
            if (radioFile.Checked)
            {
                qualeRadio = 0;
            }
            else { qualeRadio = 1; }
        }

        public string readOrigin()
        {
             return textorigine.Text;
        }
        public string readDest()
        {
            return textDest.Text;
        }

        private void textorigine_TextChanged(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textorigine.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private async void CopyBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(readOrigin()) || string.IsNullOrWhiteSpace(readDest()))
            {
                MessageBox.Show("Specificare Origine e Destinazione.", "Copia cartella", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            refresh();
            await function.copyFolder(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, readOrigin(), readDest());
        }

        private void configureCredentialMenu()
        {
            var menu = new ContextMenuStrip();
            var resetItem = new ToolStripMenuItem("Reset credenziali cifrate");
            var reimportItem = new ToolStripMenuItem("Reimport credenziali da XML legacy...");

            resetItem.Click += resetCredentialStore_Click;
            reimportItem.Click += reimportCredentials_Click;

            menu.Items.Add(resetItem);
            menu.Items.Add(reimportItem);
            btnRefresh.ContextMenuStrip = menu;
        }

        private void addCategoryAddButtons()
        {
            createAddButton(panelCMP, "CMP");
            createAddButton(panelTRD, "TRD");
            createAddButton(panelDOK, "DOK");
            createAddButton(panelSRV, "SERVER");
            createAddButton(panelGW, "GW");
            createAddButton(panelMfc, "MFC");
        }

        private void createAddButton(Panel panel, string category)
        {
            var addButton = new Button
            {
                Name = $"btnAdd{category}",
                Text = "+",
                Tag = category,
                Size = new Size(22, 22),
                Location = new Point(panel.Width - 24, 2),
                BackColor = Color.WhiteSmoke,
                FlatStyle = FlatStyle.Popup
            };
            addButton.Click += addCategoryAction_Click;
            panel.Controls.Add(addButton);
            addButton.BringToFront();
        }

        private void addCategoryAction_Click(object sender, EventArgs e)
        {
            var addButton = sender as Button;
            string category = (addButton?.Tag as string) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(category)) return;

            var menu = new ContextMenuStrip();
            var addCategoryCredentials = new ToolStripMenuItem("Aggiungi credenziali categoria");
            var addMachine = new ToolStripMenuItem("Aggiungi PC in categoria");

            addCategoryCredentials.Click += (s, a) => AddOrUpdateCategoryCredentials(category);
            addMachine.Click += (s, a) => AddMachineInCategory(category, addButton);

            menu.Items.Add(addCategoryCredentials);
            menu.Items.Add(addMachine);
            menu.Show(addButton, new Point(0, addButton.Height));
        }

        private void AddMachineInCategory(string category, Button addButton)
        {
            using (var form = new FormAddMachine(category))
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                if (!lista.AddMachine(category, form.MachineName, form.MachineIp, form.Username, form.Password, out Button machineButton, out CheckBox machineCheck, out string error))
                {
                    MessageBox.Show(error, "Inserimento macchina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Panel targetPanel = getPanelByCategory(category);
                if (targetPanel == null) return;

                AttachMachineContextMenu(machineButton);
                targetPanel.Controls.Add(machineButton);
                targetPanel.Controls.Add(machineCheck);
                addButton.BringToFront();
                relayoutPanelsAndResizeForm();
            }
        }

        private void AddOrUpdateCategoryCredentials(string category)
        {
            using (var form = new FormCategoryCredentials(category))
            {
                if (form.ShowDialog() != DialogResult.OK) return;

                lista.SetCategoryCredentials(category, form.Username, form.Password);
                AppendLog($"[{DateTime.Now:HH:mm:ss}] CATEGORY_CREDENTIALS - {category} - aggiornate.");
                MessageBox.Show("Credenziali categoria aggiornate (store cifrato).", "Credenziali", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Panel getPanelByCategory(string category)
        {
            switch ((category ?? string.Empty).ToUpperInvariant())
            {
                case "CMP": return panelCMP;
                case "TRD": return panelTRD;
                case "DOK": return panelDOK;
                case "SERVER": return panelSRV;
                case "GW": return panelGW;
                case "MFC": return panelMfc;
                default: return null;
            }
        }

        private void resetCredentialStore_Click(object sender, EventArgs e)
        {
            var answer = MessageBox.Show(
                "Confermi il reset del file credenziali cifrato?\nLe operazioni remote richiederanno reimport credenziali.",
                "Reset credenziali",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            lista.ResetCredentialStore();
            MessageBox.Show("Credenziali cifrate azzerate.", "Sicurezza", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void reimportCredentials_Click(object sender, EventArgs e)
        {
            using (var openLegacyXml = new OpenFileDialog())
            {
                openLegacyXml.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openLegacyXml.Title = "Seleziona file XML legacy con User/Password";
                if (openLegacyXml.ShowDialog() != DialogResult.OK) return;

                int migrated = lista.ReimportCredentialsFromLegacyXml(openLegacyXml.FileName, true);
                MessageBox.Show($"Credenziali importate: {migrated}", "Reimport credenziali", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void relayoutPanelsAndResizeForm()
        {
            int cmpRequired = CalculateRequiredPanelWidth(panelCMP.Height, lista.getCmpButton.Count);
            int trdRequired = CalculateRequiredPanelWidth(panelTRD.Height, lista.getTrdButton.Count);
            int srvRequired = CalculateRequiredPanelWidth(panelSRV.Height, lista.getSrvButton.Count);
            int gwRequired = CalculateRequiredPanelWidth(panelGW.Height, lista.getGwButton.Count);
            int dokRequired = CalculateRequiredPanelWidth(panelDOK.Height, lista.getDokButton.Count);
            int mfcRequired = CalculateRequiredPanelWidth(panelMfc.Height, lista.getMfcButton.Count);

            panelCMP.Width = Math.Max(panelCMP.Width, cmpRequired);
            panelTRD.Width = Math.Max(panelTRD.Width, trdRequired);
            panelSRV.Width = Math.Max(panelSRV.Width, srvRequired);
            panelGW.Width = Math.Max(panelGW.Width, gwRequired);
            panelMfc.Width = Math.Max(panelMfc.Width, mfcRequired);

            panelTRD.Left = panelCMP.Right + PanelGap;
            panelGW.Left = panelSRV.Right + PanelGap;
            panelDOK.Width = Math.Max(dokRequired, panelGW.Right - panelDOK.Left);
            panelMfc.Left = panelDOK.Right + PanelGap;

            MoveRightControls(Math.Max(0, (panelTRD.Right + 20) - BaseRightControlsStartX));

            LayoutCategoryPanel(panelCMP, lista.getCmpButton, lista.getCmpCheck);
            LayoutCategoryPanel(panelTRD, lista.getTrdButton, lista.getTrdCheck);
            LayoutCategoryPanel(panelSRV, lista.getSrvButton, lista.getSrvCheck);
            LayoutCategoryPanel(panelGW, lista.getGwButton, lista.getGwCheck);
            LayoutCategoryPanel(panelDOK, lista.getDokButton, lista.getDokCheck);
            LayoutCategoryPanel(panelMfc, lista.getMfcButton, lista.getMfcCheck);

            PlaceAddButton(panelCMP, "btnAddCMP");
            PlaceAddButton(panelTRD, "btnAddTRD");
            PlaceAddButton(panelDOK, "btnAddDOK");
            PlaceAddButton(panelSRV, "btnAddSERVER");
            PlaceAddButton(panelGW, "btnAddGW");
            PlaceAddButton(panelMfc, "btnAddMFC");
            setupPanelTitles();

            int rightMost = Math.Max(panelMfc.Right, panel1.Right);
            rightMost = Math.Max(rightMost, textBoxErr.Right);
            rightMost = Math.Max(rightMost, textDest.Right);
            rightMost = Math.Max(rightMost, CopyBtn.Right);

            this.ClientSize = new Size(rightMost + 12, this.ClientSize.Height);
        }

        private int CalculateRequiredPanelWidth(int panelHeight, int itemCount)
        {
            int rowsPerColumn = Math.Max(1, (panelHeight - 28) / 30);
            int columns = Math.Max(1, (int)Math.Ceiling(itemCount / (double)rowsPerColumn));
            return 5 + (columns * 105) + AddButtonReservedWidth;
        }

        private void LayoutCategoryPanel(Panel panel, IList<Button> buttons, IList<CheckBox> checks)
        {
            int rowsPerColumn = Math.Max(1, (panel.Height - 28) / 30);

            for (int i = 0; i < buttons.Count; i++)
            {
                Button btn = buttons[i] as Button;
                CheckBox chk = checks[i] as CheckBox;
                if (btn == null || chk == null) continue;

                int column = i / rowsPerColumn;
                int row = i % rowsPerColumn;
                int x = 5 + (column * 105);
                int y = 24 + (row * 30);

                btn.Location = new Point(x, y);
                chk.Location = new Point(x + 85, y + 10);
            }
        }

        private void MoveRightControls(int deltaX)
        {
            label1.Left = BaseRightControlsStartX + deltaX;
            textorigine.Left = BaseRightControlsStartX + 3 + deltaX;
            label2.Left = BaseRightControlsStartX + deltaX;
            textDest.Left = BaseRightControlsStartX + 3 + deltaX;
            CopyBtn.Left = BaseRightControlsStartX + 57 + deltaX;
            textBoxErr.Left = 847 + deltaX;
            panel1.Left = 1005 + deltaX;
        }

        private void PlaceAddButton(Panel panel, string buttonName)
        {
            Control[] found = panel.Controls.Find(buttonName, false);
            if (found.Length == 0) return;

            var add = found[0] as Button;
            if (add == null) return;

            add.Location = new Point(panel.Width - add.Width - 2, 2);
            add.BringToFront();
        }

        private void AppendLog(string message)
        {
            if (textBoxErr == null || textBoxErr.IsDisposed) return;

            if (textBoxErr.InvokeRequired)
            {
                textBoxErr.BeginInvoke((Action)(() => AppendLog(message)));
                return;
            }

            textBoxErr.AppendText(message + Environment.NewLine);
        }

        private void setupPanelTitles()
        {
            EnsurePanelHeader(panelCMP, "CMP");
            EnsurePanelHeader(panelTRD, "TRD");
            EnsurePanelHeader(panelDOK, "DOK");
            EnsurePanelHeader(panelSRV, "SERVER");
            EnsurePanelHeader(panelGW, "GW");
            EnsurePanelHeader(panelMfc, "MFC");
        }

        private void EnsurePanelHeader(Panel panel, string title)
        {
            string labelName = "lblHeader" + title;
            Control[] existing = panel.Controls.Find(labelName, false);
            Label label;
            if (existing.Length == 0)
            {
                label = new Label
                {
                    Name = labelName,
                    AutoSize = true,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                panel.Controls.Add(label);
            }
            else
            {
                label = existing[0] as Label;
            }

            if (label == null) return;
            label.Text = title;
            label.Location = new Point(6, 5);
            label.BringToFront();
        }

        private void wireMachineContextMenus()
        {
            foreach (Button btn in lista.getlistaButton)
            {
                AttachMachineContextMenu(btn);
            }
        }

        private void AttachMachineContextMenu(Button btn)
        {
            if (btn == null) return;
            if (btn.ContextMenuStrip != null) return;

            var menu = new ContextMenuStrip();
            var openExplorer = new ToolStripMenuItem("File Explorer");
            var openRdp = new ToolStripMenuItem("Desktop Remoto");
            var deleteMachine = new ToolStripMenuItem("Elimina macchina");

            openExplorer.Click += MachineOpenExplorer_Click;
            openRdp.Click += MachineOpenRdp_Click;
            deleteMachine.Click += MachineDelete_Click;

            menu.Items.Add(openExplorer);
            menu.Items.Add(openRdp);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(deleteMachine);
            btn.ContextMenuStrip = menu;
        }

        private Button ResolveMachineButtonFromMenuSender(object sender)
        {
            if (sender is ToolStripItem item &&
                item.Owner is ContextMenuStrip cms &&
                cms.SourceControl is Button btn)
            {
                return btn;
            }

            return null;
        }

        private async void MachineOpenExplorer_Click(object sender, EventArgs e)
        {
            var btn = ResolveMachineButtonFromMenuSender(sender);
            if (btn == null) return;
            await function.openFolder(lista.getlistamacchine, btn);
        }

        private async void MachineOpenRdp_Click(object sender, EventArgs e)
        {
            var btn = ResolveMachineButtonFromMenuSender(sender);
            if (btn == null) return;
            await function.openRmDesk(lista.getlistamacchine, btn);
        }

        private void MachineDelete_Click(object sender, EventArgs e)
        {
            var btn = ResolveMachineButtonFromMenuSender(sender);
            if (btn == null) return;

            var answer = MessageBox.Show(
                $"Eliminare la macchina {btn.Name}?",
                "Conferma eliminazione",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes) return;

            if (!lista.RemoveMachine(btn.Name, out string error))
            {
                MessageBox.Show(error, "Elimina macchina", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (btn.Parent != null)
            {
                var checkbox = btn.Parent.Controls.OfType<CheckBox>()
                    .FirstOrDefault(c => string.Equals(c.Name, btn.Name, StringComparison.OrdinalIgnoreCase));
                if (checkbox != null) btn.Parent.Controls.Remove(checkbox);
                btn.Parent.Controls.Remove(btn);
            }

            AppendLog($"[{DateTime.Now:HH:mm:ss}] DELETE_MACHINE - {btn.Name} - rimossa.");
            relayoutPanelsAndResizeForm();
        }
    }
}
