using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace HighLandirect.Views
{
    public partial class CustomerListView : UserControl
    {
        public CustomerListView()
        {
            InitializeComponent();
        }

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var border = VisualTreeHelper.GetChild((DataGrid)sender, 0) as Decorator;
            if (border != null)
            {
                var scrollViewer = border.Child as ScrollViewer;
                scrollViewer.ScrollToTop();
            }
        }

        private void UserControl_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //
        }
    }
}
