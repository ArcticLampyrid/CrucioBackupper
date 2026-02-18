using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CrucioBackupper.LogViewer;

public partial class LogViewerControl : UserControl
{
    private LogViewModel? observedViewModel;

    public LogViewerControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        AttachCollectionListener();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        AttachCollectionListener();
    }

    private void AttachCollectionListener()
    {
        if (observedViewModel != null)
        {
            observedViewModel.Entities.CollectionChanged -= OnEntitiesCollectionChanged;
        }

        observedViewModel = DataContext as LogViewModel;
        if (observedViewModel != null)
        {
            observedViewModel.Entities.CollectionChanged += OnEntitiesCollectionChanged;
        }
    }

    private void OnEntitiesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (CanAutoScroll.IsChecked != true)
        {
            return;
        }

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is { Count: > 0 })
        {
            var item = e.NewItems[e.NewItems.Count - 1];
            if (item != null)
            {
                LogListBox.ScrollIntoView(item);
            }
        }
    }

    private async void LogListBox_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (LogListBox.SelectedItem is not LogEntity entity)
        {
            return;
        }

        if (VisualRoot is not Window owner)
        {
            return;
        }

        var details = $"{entity.Timestamp:s} {entity.Level}: {entity.Message}";
        if (entity.HasException)
        {
            details += Environment.NewLine + entity.Exception;
        }

        await MessageBoxManager.GetMessageBoxStandard(
            "Log Message",
            details,
            ButtonEnum.Ok).ShowWindowDialogAsync(owner);
    }
}
