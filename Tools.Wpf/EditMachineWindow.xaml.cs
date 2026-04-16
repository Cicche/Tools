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
        public bool DeleteRequested { get; private set; }
        public string Category => (CmbCategory.SelectedItem as string ?? string.Empty).Trim().ToUpperInvariant();
        public string MachineName => TxtName.Text.Trim();
        public string Ip => TxtIp.Text.Trim();
        public string User => TxtUser.Text.Trim();
        public string Password => TxtPassword.Password;

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
            if (TxtConfirmPassword.Password == TxtPassword.Password)
            {
                LblValidation.Text = string.Empty;
                return;
            }

            LblValidation.Text = "Le password non corrispondono.";
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

            if (hasPassword && TxtConfirmPassword.Password != TxtPassword.Password)
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
