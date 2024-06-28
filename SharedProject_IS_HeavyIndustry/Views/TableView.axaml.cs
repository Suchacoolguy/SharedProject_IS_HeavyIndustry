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
    
    /*private async void OnFilterButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Get the Button that was clicked
        var button = sender as Button;
        
        // Get the DataGridTextColumn from the Button's DataContext
        var column = button?.DataContext as String;
        
        // var window = new (distinctAssem);
        // var parentWindow = this.GetVisualRoot() as Window;
        // var result = await window.ShowDialog<ObservableCollection<bool>>(parentWindow);
    
        if (column != null)
        {
            // 여기서 처리하자   
        }
    }*/
    
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
        Console.WriteLine($"Column header: {columnHeader}");
    }

    private void FilterApply_Btn_Click(object? sender, RoutedEventArgs e)
    {
        // ListBox 참조
        /*var filterListBox = this.FindControl<ListBox>("FilterListBox");
        if (filterListBox != null)
        {
            var selectedItems = filterListBox.SelectedItems!
                .OfType<string>()
                .ToList();

            MainWindowViewModel.BomDataViewModel.SelectedFilterItems = new ObservableCollection<string>(selectedItems);
        }*/
        Console.WriteLine("hello");
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

            // Refresh the DataGrid to reflect changes
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

            // Refresh the DataGrid to reflect changes
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

            // Refresh the DataGrid to reflect changes
            table.ItemsSource = new ObservableCollection<Part>(table.ItemsSource.Cast<Part>());
        }
    }
}


/*private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var listBox = sender as ListBox;
    var viewModel = DataContext as BOMDataViewModel;

    if (listBox != null && viewModel != null)
    {
        viewModel.SelectedFilterItems.Clear();
        foreach (var item in listBox.SelectedItems)
        {
            viewModel.SelectedFilterItems.Add(item.ToString());
        }
    }
}*/