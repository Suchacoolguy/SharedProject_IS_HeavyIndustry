using System;
using System.Collections.ObjectModel;
using System.Linq;
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

    private void Search_btn_click(object? sender, RoutedEventArgs e)
    {
        var input = this.FindControl<TextBox>("SearchBox")!.Text ?? "";
        RawStandardViewModel.LengthSetList.Clear();
        foreach (var value in RawStandardViewModel.TempLengthSetList)
            if(value.Description.Contains(input!))
                RawStandardViewModel.LengthSetList.Add(value);
    }
    
    /*private void Search_btn_click(object? sender, RoutedEventArgs e)
    {
        var input = this.FindControl<TextBox>("SearchBox")!.Text;
        var dataGrid = this.FindControl<DataGrid>("Table")!;
        var items = (ObservableCollection<RawLengthSet>)dataGrid.ItemsSource;

        if (items == null) return;

        // `input` 문자열을 포함하는 항목들을 찾기
        var matchingItems = items.Where(item => item.Description.Contains(input)).ToList();

        // 기존 컬렉션에서 matchingItems 제거
        foreach (var item in matchingItems)
        {
            items.Remove(item);
        }

        // matchingItems를 컬렉션의 맨 앞에 추가
        for (int i = matchingItems.Count - 1; i >= 0; i--)
        {
            items.Insert(0, matchingItems[i]);
        }
    }*/


    private void Filter_Btn_Click(object? sender, RoutedEventArgs e)
    {
        GenerateFilter();
    }

    private void GenerateFilter()
    {
        throw new NotImplementedException();
    }
}