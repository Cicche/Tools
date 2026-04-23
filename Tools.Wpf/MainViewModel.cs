using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tools.Wpf
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string xmlPath = "Tool_List.xml";
        private string copyOrigin = string.Empty;
        private string copyDestination = "C$\\Example\\temp";
        private string timeoutMs = "8000";
        private string retryCount = "3";
        private double categoriesContainerMinWidth = 780;
        private double categoriesContainerMinHeight = 420;
        private CategoryPanelModel obtsCategory;

        public ObservableCollection<CategoryPanelModel> Categories { get; } = new ObservableCollection<CategoryPanelModel>();
        public ObservableCollection<CategoryRowModel> CategoryRows { get; } = new ObservableCollection<CategoryRowModel>();

        public CategoryPanelModel ObtsCategory
        {
            get => obtsCategory;
            set
            {
                if (ReferenceEquals(obtsCategory, value)) return;
                obtsCategory = value;
                OnPropertyChanged();
            }
        }

        public string XmlPath
        {
            get => xmlPath;
            set
            {
                if (xmlPath == value) return;
                xmlPath = value;
                OnPropertyChanged();
            }
        }

        public string CopyOrigin
        {
            get => copyOrigin;
            set
            {
                if (copyOrigin == value) return;
                copyOrigin = value;
                OnPropertyChanged();
            }
        }

        public string CopyDestination
        {
            get => copyDestination;
            set
            {
                if (copyDestination == value) return;
                copyDestination = value;
                OnPropertyChanged();
            }
        }

        public string TimeoutMs
        {
            get => timeoutMs;
            set
            {
                if (timeoutMs == value) return;
                timeoutMs = value;
                OnPropertyChanged();
            }
        }

        public string RetryCount
        {
            get => retryCount;
            set
            {
                if (retryCount == value) return;
                retryCount = value;
                OnPropertyChanged();
            }
        }

        public double CategoriesContainerMinWidth
        {
            get => categoriesContainerMinWidth;
            set
            {
                if (categoriesContainerMinWidth.Equals(value)) return;
                categoriesContainerMinWidth = value;
                OnPropertyChanged();
            }
        }

        public double CategoriesContainerMinHeight
        {
            get => categoriesContainerMinHeight;
            set
            {
                if (categoriesContainerMinHeight.Equals(value)) return;
                categoriesContainerMinHeight = value;
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
