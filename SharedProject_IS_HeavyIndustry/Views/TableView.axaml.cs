using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class TableView : UserControl
{
    public TableView()
    {
        InitializeComponent(); 
    }
    
    // FilterButton_Click
    private void FilterButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
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