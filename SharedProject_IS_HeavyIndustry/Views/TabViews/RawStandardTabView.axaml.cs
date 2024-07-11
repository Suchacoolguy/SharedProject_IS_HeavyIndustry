using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class RawStandardTabView : TabView
{
    public RawStandardTabView()
    {
        InitializeComponent();
        DataContext = new RawStandardViewModel();
    }

    private void MoveToBottum(object? sender, RoutedEventArgs routedEventArgs)
    {
        var dataGrid = this.FindControl<DataGrid>("Table")!;
        var items = (ObservableCollection<RawLengthSet>)dataGrid.ItemsSource;
        items.Add(new RawLengthSet("", 0, ""));
        if (items.Count > 0)
        {
            dataGrid.ScrollIntoView(items[items.Count - 1], null);
        }
    }

    private void Filter_Btn_Click(object? sender, RoutedEventArgs e)
    {
        GenerateFilter();
    }

    private void GenerateFilter()
    {
        throw new NotImplementedException();
    }
}