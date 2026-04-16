using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace Tools
{
    internal class FormAddMachine : Form
    {
        private readonly TextBox txtName;
        private readonly TextBox txtIp;
        private readonly TextBox txtUser;
        private readonly TextBox txtPassword;
        private readonly TextBox txtPasswordConfirm;
        private readonly Label lblPasswordMismatch;
        private readonly Label lblCategoryValue;

        public string Category { get; }
        public string MachineName => txtName.Text.Trim();
        public string MachineIp => txtIp.Text.Trim();
        public string Username => txtUser.Text.Trim();
        public string Password => txtPassword.Text;

        public FormAddMachine(string category)
        {
            Category = (category ?? string.Empty).Trim().ToUpperInvariant();

            Text = "Nuova macchina";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 280);

            var lblCategory = new Label { Left = 12, Top = 18, Width = 120, Text = "Categoria" };
            lblCategoryValue = new Label { Left = 140, Top = 18, Width = 250, Text = Category };
            var lblName = new Label { Left = 12, Top = 52, Width = 120, Text = "Nome macchina" };
            var lblIp = new Label { Left = 12, Top = 86, Width = 120, Text = "IP" };
            var lblUser = new Label { Left = 12, Top = 120, Width = 120, Text = "User" };
            var lblPassword = new Label { Left = 12, Top = 154, Width = 120, Text = "Password" };
            var lblPasswordConfirm = new Label { Left = 12, Top = 188, Width = 120, Text = "Conferma pwd" };

            txtName = new TextBox { Left = 140, Top = 48, Width = 250 };
            txtIp = new TextBox { Left = 140, Top = 82, Width = 250 };
            txtUser = new TextBox { Left = 140, Top = 116, Width = 250 };
            txtPassword = new TextBox { Left = 140, Top = 150, Width = 250, UseSystemPasswordChar = true };
            txtPasswordConfirm = new TextBox { Left = 140, Top = 184, Width = 250, UseSystemPasswordChar = true };
            txtPassword.TextChanged += PasswordFieldsChanged;
            txtPasswordConfirm.TextChanged += PasswordFieldsChanged;

            lblPasswordMismatch = new Label
            {
                Left = 140,
                Top = 214,
                Width = 250,
                ForeColor = Color.Red,
                Text = "Password non corrispondenti",
                Visible = false
            };

            var btnCancel = new Button
            {
                Text = "Annulla",
                Left = 232,
                Top = 240,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            var btnOk = new Button
            {
                Text = "Salva",
                Left = 315,
                Top = 240,
                Width = 75
            };
            btnOk.Click += BtnOk_Click;

            Controls.Add(lblCategory);
            Controls.Add(lblCategoryValue);
            Controls.Add(lblName);
            Controls.Add(lblIp);
            Controls.Add(lblUser);
            Controls.Add(lblPassword);
            Controls.Add(lblPasswordConfirm);
            Controls.Add(txtName);
            Controls.Add(txtIp);
            Controls.Add(txtUser);
            Controls.Add(txtPassword);
            Controls.Add(txtPasswordConfirm);
            Controls.Add(lblPasswordMismatch);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MachineName) ||
                string.IsNullOrWhiteSpace(MachineIp) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(txtPasswordConfirm.Text))
            {
                MessageBox.Show("Compilare tutti i campi.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(Password, txtPasswordConfirm.Text, StringComparison.Ordinal))
            {
                lblPasswordMismatch.Visible = true;
                return;
            }

            if (!IPAddress.TryParse(MachineIp, out _))
            {
                MessageBox.Show("IP non valido.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void PasswordFieldsChanged(object sender, EventArgs e)
        {
            lblPasswordMismatch.Visible =
                !string.IsNullOrEmpty(txtPassword.Text) &&
                !string.IsNullOrEmpty(txtPasswordConfirm.Text) &&
                !string.Equals(txtPassword.Text, txtPasswordConfirm.Text, StringComparison.Ordinal);
        }
    }
}
