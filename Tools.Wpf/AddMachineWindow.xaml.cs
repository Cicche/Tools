using System.Net;
using System.Windows;

namespace Tools.Wpf
{
    public partial class AddMachineWindow : Window
    {
        public string Category => TxtCategory.Text.Trim().ToUpperInvariant();
        public string MachineName => TxtName.Text.Trim();
        public string Ip => TxtIp.Text.Trim();
        public string User => TxtUser.Text.Trim();
        public string Password => TxtPassword.Password;

        public AddMachineWindow(string category, string machineName = null, string ip = null)
        {
            InitializeComponent();
            TxtCategory.Text = (category ?? string.Empty).Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(machineName))
            {
                TxtName.Text = machineName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(ip))
            {
                TxtIp.Text = ip.Trim();
            }
        }

        private void TxtConfirmPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (TxtConfirmPassword.Password == TxtPassword.Password)
            {
                LblValidation.Text = string.Empty;
                return;
            }

            LblValidation.Text = "Le password non corrispondono.";
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MachineName) ||
                string.IsNullOrWhiteSpace(Ip))
            {
                LblValidation.Text = "Compilare almeno Nome macchina e IP.";
                return;
            }

            if (!IPAddress.TryParse(Ip, out _))
            {
                LblValidation.Text = "IP non valido.";
                return;
            }

            bool hasUser = !string.IsNullOrWhiteSpace(User);
            bool hasPassword = !string.IsNullOrWhiteSpace(Password);
            if (hasUser != hasPassword)
            {
                LblValidation.Text = "User e Password devono essere entrambi valorizzati o entrambi vuoti.";
                return;
            }

            if (hasPassword && TxtConfirmPassword.Password != TxtPassword.Password)
            {
                LblValidation.Text = "Le password non corrispondono.";
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
