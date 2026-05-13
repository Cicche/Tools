using System.Net;
using System.Windows;

namespace Tools.Wpf
{
    public partial class AddMachineWindow : Window
    {
        private bool syncingPasswordFields;
        public string Category => TxtCategory.Text.Trim().ToUpperInvariant();
        public string MachineName => TxtName.Text.Trim();
        public string Ip => TxtIp.Text.Trim();
        public string User => TxtUser.Text.Trim();
        public string Password => ChkShowPassword.IsChecked == true ? TxtPasswordVisible.Text : TxtPassword.Password;

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
            if (syncingPasswordFields) return;
            if (ChkShowPassword.IsChecked == true)
            {
                syncingPasswordFields = true;
                TxtConfirmPasswordVisible.Text = TxtConfirmPassword.Password;
                syncingPasswordFields = false;
            }

            if (GetConfirmPassword() == Password)
            {
                LblValidation.Text = string.Empty;
                return;
            }

            LblValidation.Text = "Le password non corrispondono.";
        }

        private void TxtPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (syncingPasswordFields) return;
            if (ChkShowPassword.IsChecked == true)
            {
                syncingPasswordFields = true;
                TxtPasswordVisible.Text = TxtPassword.Password;
                syncingPasswordFields = false;
            }
        }

        private void TxtPasswordVisible_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (syncingPasswordFields || ChkShowPassword.IsChecked != true) return;
            syncingPasswordFields = true;
            TxtPassword.Password = TxtPasswordVisible.Text;
            syncingPasswordFields = false;
        }

        private void TxtConfirmPasswordVisible_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (syncingPasswordFields || ChkShowPassword.IsChecked != true) return;
            syncingPasswordFields = true;
            TxtConfirmPassword.Password = TxtConfirmPasswordVisible.Text;
            syncingPasswordFields = false;
        }

        private void ChkShowPassword_OnToggled(object sender, RoutedEventArgs e)
        {
            bool show = ChkShowPassword.IsChecked == true;
            syncingPasswordFields = true;
            if (show)
            {
                TxtPasswordVisible.Text = TxtPassword.Password;
                TxtConfirmPasswordVisible.Text = TxtConfirmPassword.Password;
            }
            else
            {
                TxtPassword.Password = TxtPasswordVisible.Text;
                TxtConfirmPassword.Password = TxtConfirmPasswordVisible.Text;
            }

            TxtPassword.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            TxtConfirmPassword.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            TxtPasswordVisible.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            TxtConfirmPasswordVisible.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            syncingPasswordFields = false;
        }

        private string GetConfirmPassword()
        {
            return ChkShowPassword.IsChecked == true ? TxtConfirmPasswordVisible.Text : TxtConfirmPassword.Password;
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

            if (hasPassword && GetConfirmPassword() != Password)
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
