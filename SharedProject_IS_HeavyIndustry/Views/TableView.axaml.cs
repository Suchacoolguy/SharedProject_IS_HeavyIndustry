using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class TableView : UserControl
{
    private Dictionary<string, ContextMenu> filterSet = new();
    public TableView()
    {
        InitializeComponent();
    }
    
    [Obsolete("Obsolete")]
    private void Filter_Btn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        var columnHeader = button.Tag?.ToString();
        ContextMenu contextMenu;

        if (filterSet.TryGetValue(columnHeader!, out var value))
            contextMenu = value;
        else
        {
            contextMenu = FilteringService.GenerateFilter(columnHeader!);
            filterSet.Add(columnHeader!, contextMenu);
        }

        contextMenu.PlacementTarget = button;
        contextMenu.Open(button);
    }
    
    public void ToggleNeedSeperate(bool value)
    {
        var table = this.FindControl<DataGrid>("Table");

        if (table != null)
        {
            foreach (var item in table.ItemsSource.Cast<Part>())
            {
                if (item.IsOverLenth)
                    item.NeedSeparate = value;
            }
            
            table.ItemsSource = new ObservableCollection<Part>(table.ItemsSource.Cast<Part>());
        }
    }

    public void SetExcludeFalse()
    {
        var table = this.FindControl<DataGrid>("Table");

        if (table != null)
        {
            foreach (var item in table.ItemsSource.Cast<Part>())
            {
                if (item.IsExcluded)
                    item.IsExcluded = false;
            }
            
            table.ItemsSource = new ObservableCollection<Part>(table.ItemsSource.Cast<Part>());
        }
    }
    
    public void SetExcludeTrue()
    {
        var table = this.FindControl<DataGrid>("Table");

        if (table != null)
        {
            foreach (var item in table.ItemsSource.Cast<Part>())
            {
                if (item.IsExcluded)
                    item.IsExcluded = true;
            }
            
            table.ItemsSource = new ObservableCollection<Part>(table.ItemsSource.Cast<Part>());
        }
    }
}
