using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Tools;

namespace Tools.Wpf
{
    public class MachineRow : INotifyPropertyChanged
    {
        private Brush background = Brushes.LightGray;
        private bool isBusy;
        private bool isSelected;
        private string status = string.Empty;

        public PC Source { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public string CredentialState { get; set; }

        public Brush Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
