using System.Collections.Generic;
using System.Windows.Controls;

namespace Finance_Manager.cs
{
    public static class GuiHelper
    {
        public static void SetDataGridItemsSource<T>(DataGrid dataGrid, List<T> itemsSource)
        {
            dataGrid.ItemsSource = itemsSource;
        }
    }
}
