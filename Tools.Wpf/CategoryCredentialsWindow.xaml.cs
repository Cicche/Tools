using System.Windows;

namespace Tools.Wpf
{
    public partial class CategoryCredentialsWindow : Window
    {
        public string Category => TxtCategory.Text.Trim().ToUpperInvariant();
        public string User => TxtUser.Text.Trim();
        public string Password => TxtPassword.Password;

        public CategoryCredentialsWindow(string category)
        {
            InitializeComponent();
            TxtCategory.Text = (category ?? string.Empty).Trim().ToUpperInvariant();
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
            if (string.IsNullOrWhiteSpace(User) || string.IsNullOrWhiteSpace(Password))
            {
                LblValidation.Text = "Compilare tutti i campi.";
                return;
            }

            if (TxtConfirmPassword.Password != TxtPassword.Password)
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
