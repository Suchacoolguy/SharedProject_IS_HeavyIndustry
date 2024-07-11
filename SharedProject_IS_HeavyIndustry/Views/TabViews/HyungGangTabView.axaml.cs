using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class HyungGangTabView : TabView
{
    public HyungGangTabView()
    {
        InitializeComponent();
        DataContext = new HyungGangViewModel();
    }
    
    private void MoveToBottum(object? sender, RoutedEventArgs routedEventArgs)
    {
        var dataGrid = this.FindControl<DataGrid>("Table")!;
        var items = (ObservableCollection<HyungGang>)dataGrid.ItemsSource;
        var newItem = new HyungGang("", "");
        items.Add(newItem);
        
        if (items.Count > 0)
        {
            dataGrid.ScrollIntoView(items[items.Count - 1], null);
            dataGrid.SelectedItem = newItem; // 새로 추가된 항목을 선택하여 강조 표시
        }
    }
}