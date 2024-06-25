using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class TableView : UserControl
{
    public TableView()
    {
        InitializeComponent(); 
    }
    
    // FilterButton_Click

    // private async void OnFilterButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    // {
    //     // Get the Button that was clicked
    //     var button = sender as Button;
    //     
    //     // Get the DataGridTextColumn from the Button's DataContext
    //     var column = button?.DataContext as String;
    //     
    //     List<string> distinctAssem = BOMDataViewModel.PartsFiltered.Select(p => p.Assem).Distinct().ToList();
    //     BOMDataViewModel.OptionsForFiltering = distinctAssem;
    //     
    //     var window = new SelectAssemWindow(distinctAssem);
    //     var parentWindow = this.GetVisualRoot() as Window;
    //     var result = await window.ShowDialog<ObservableCollection<bool>>(parentWindow);
    //
    //     if (column != null)
    //     {
    //         // 여기서 처리하자   
    //     }
    // }
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