using System.Windows;

namespace Tools.Wpf
{
    public partial class CategoryCredentialsWindow : Window
    {
        private bool syncingPasswordFields;
        public string Category => TxtCategory.Text.Trim().ToUpperInvariant();
        public string User => TxtUser.Text.Trim();
        public string Password => ChkShowPassword.IsChecked == true ? TxtPasswordVisible.Text : TxtPassword.Password;

        public CategoryCredentialsWindow(string category)
        {
            InitializeComponent();
            TxtCategory.Text = (category ?? string.Empty).Trim().ToUpperInvariant();
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
            if (string.IsNullOrWhiteSpace(User) || string.IsNullOrWhiteSpace(Password))
            {
                LblValidation.Text = "Compilare tutti i campi.";
                return;
            }

            if (GetConfirmPassword() != Password)
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
