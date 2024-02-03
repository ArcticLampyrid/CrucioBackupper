using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CrucioBackupper.LogViewer
{
    public partial class LogViewerControl : UserControl
    {
        public LogViewerControl()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)ListView.Items).CollectionChanged += OnListViewItemsChanged;
        }

        private void OnListViewItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!CanAutoScroll.IsChecked == true)
                return;
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                if (e.NewItems.Count != 0)
                {
                    var newItem = e.NewItems[e.NewItems.Count - 1];
                    ListView.ScrollIntoView(newItem);
                }
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;
            while ((originalSource != null) && originalSource is not ListViewItem)
                originalSource = VisualTreeHelper.GetParent(originalSource);
            if (originalSource is ListViewItem item)
            {
                if (item.DataContext is LogEntity entity)
                {
                    MessageBox.Show($"{entity.Timestamp:s} {entity.Level}: {entity.Message}{Environment.NewLine}{entity.Exception}",
                        "Log Message",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }
    }
}
