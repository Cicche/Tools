using System.Collections.ObjectModel;

namespace Tools.Wpf
{
    public class CategoryPanelModel
    {
        public string Name { get; set; }
        public int RowsPerColumn { get; set; } = 6;
        public int ColumnCount { get; set; } = 1;
        public int VisibleRowCount { get; set; } = 1;
        public double PanelMinHeight { get; set; } = 90;
        public double PanelMinWidth { get; set; } = 180;
        public double ItemsHostHeight { get; set; } = 32;
        public ObservableCollection<MachineRow> Machines { get; } = new ObservableCollection<MachineRow>();
    }
}
