using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tools
{
    internal class FormCategoryCredentials : Form
    {
        private readonly TextBox txtUser;
        private readonly TextBox txtPassword;
        private readonly TextBox txtPasswordConfirm;
        private readonly Label lblMismatch;

        public string Category { get; }
        public string Username => txtUser.Text.Trim();
        public string Password => txtPassword.Text;

        public FormCategoryCredentials(string category)
        {
            Category = (category ?? string.Empty).Trim().ToUpperInvariant();

            Text = "Credenziali categoria";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 215);

            var lblCategory = new Label { Left = 12, Top = 18, Width = 120, Text = "Categoria" };
            var lblCategoryValue = new Label { Left = 140, Top = 18, Width = 250, Text = Category };
            var lblUser = new Label { Left = 12, Top = 52, Width = 120, Text = "User" };
            var lblPassword = new Label { Left = 12, Top = 86, Width = 120, Text = "Password" };
            var lblPasswordConfirm = new Label { Left = 12, Top = 120, Width = 120, Text = "Conferma pwd" };

            txtUser = new TextBox { Left = 140, Top = 48, Width = 250 };
            txtPassword = new TextBox { Left = 140, Top = 82, Width = 250, UseSystemPasswordChar = true };
            txtPasswordConfirm = new TextBox { Left = 140, Top = 116, Width = 250, UseSystemPasswordChar = true };
            txtPassword.TextChanged += PasswordChanged;
            txtPasswordConfirm.TextChanged += PasswordChanged;

            lblMismatch = new Label
            {
                Left = 140,
                Top = 146,
                Width = 250,
                ForeColor = Color.Red,
                Text = "Password non corrispondenti",
                Visible = false
            };

            var btnCancel = new Button
            {
                Text = "Annulla",
                Left = 232,
                Top = 175,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            var btnOk = new Button
            {
                Text = "Salva",
                Left = 315,
                Top = 175,
                Width = 75
            };
            btnOk.Click += BtnOk_Click;

            Controls.Add(lblCategory);
            Controls.Add(lblCategoryValue);
            Controls.Add(lblUser);
            Controls.Add(lblPassword);
            Controls.Add(lblPasswordConfirm);
            Controls.Add(txtUser);
            Controls.Add(txtPassword);
            Controls.Add(txtPasswordConfirm);
            Controls.Add(lblMismatch);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(txtPasswordConfirm.Text))
            {
                MessageBox.Show("Compilare tutti i campi.", "Dati mancanti", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(Password, txtPasswordConfirm.Text, StringComparison.Ordinal))
            {
                lblMismatch.Visible = true;
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void PasswordChanged(object sender, EventArgs e)
        {
            lblMismatch.Visible =
                !string.IsNullOrWhiteSpace(txtPassword.Text) &&
                !string.IsNullOrWhiteSpace(txtPasswordConfirm.Text) &&
                !string.Equals(txtPassword.Text, txtPasswordConfirm.Text, StringComparison.Ordinal);
        }
    }
}
