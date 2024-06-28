using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

    public void SetExclude(bool value)
    {
        var table = this.FindControl<DataGrid>("Table");

        if (table == null) return;
        foreach (var item in table.ItemsSource.Cast<Part>())
            item.IsExcluded = value;
    }
    
    public void SetSeparate(bool value)
    {
        var table = this.FindControl<DataGrid>("Table");

        if (table == null) return;
        foreach (var item in table.ItemsSource.Cast<Part>())
            item.NeedSeparate = value;
    }
}
