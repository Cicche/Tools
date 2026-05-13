using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Tools;

namespace Tools.Wpf
{
    public partial class EditMachineWindow : Window
    {
        private bool syncingPasswordFields;
        public bool DeleteRequested { get; private set; }
        public string Category => (CmbCategory.SelectedItem as string ?? string.Empty).Trim().ToUpperInvariant();
        public string MachineName => TxtName.Text.Trim();
        public string Ip => TxtIp.Text.Trim();
        public string User => TxtUser.Text.Trim();
        public string Password => ChkShowPassword.IsChecked == true ? TxtPasswordVisible.Text : TxtPassword.Password;

        public EditMachineWindow(PC machine, IEnumerable<string> categories)
        {
            InitializeComponent();

            List<string> categoryList = (categories ?? Array.Empty<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            CmbCategory.ItemsSource = categoryList;

            string currentCategory = (machine?.Type ?? string.Empty).Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(currentCategory) && !categoryList.Contains(currentCategory, StringComparer.OrdinalIgnoreCase))
            {
                categoryList.Add(currentCategory);
                CmbCategory.ItemsSource = categoryList;
            }
            CmbCategory.SelectedItem = string.IsNullOrWhiteSpace(currentCategory) ? categoryList.FirstOrDefault() : currentCategory;

            TxtName.Text = machine?.Nome ?? string.Empty;
            TxtIp.Text = machine?.Ip ?? string.Empty;
            TxtUser.Text = machine?.User ?? string.Empty;
            TxtPassword.Password = machine?.Password ?? string.Empty;
            TxtConfirmPassword.Password = machine?.Password ?? string.Empty;
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
            if (string.IsNullOrWhiteSpace(Category) ||
                string.IsNullOrWhiteSpace(MachineName) ||
                string.IsNullOrWhiteSpace(Ip))
            {
                LblValidation.Text = "Categoria, Nome macchina e IP sono obbligatori.";
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

            DeleteRequested = false;
            DialogResult = true;
            Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult first = MessageBox.Show(
                "Confermi eliminazione macchina?",
                "Conferma",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (first != MessageBoxResult.Yes) return;

            MessageBoxResult second = MessageBox.Show(
                "Eliminazione definitiva. Continuare?",
                "Conferma finale",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (second != MessageBoxResult.Yes) return;

            DeleteRequested = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested = false;
            DialogResult = false;
            Close();
        }
    }
}
