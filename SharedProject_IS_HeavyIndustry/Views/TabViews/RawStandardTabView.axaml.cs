using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;
using SharedProject_IS_HeavyIndustry.Converters;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
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
        var newItem = new RawLengthSet("", 0, "");
        RawStandardViewModel.LengthSetList.Add(newItem);
        items.Add(newItem);
        if (items.Count > 0)
        {
            dataGrid.ScrollIntoView(items[items.Count - 1], null);
        }
    }

    private void deleteItem(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (Table.ItemsSource is ObservableCollection<RawLengthSet> items)
        {
            // Copy the selected items to a list to avoid modifying the collection while iterating
            var selectedItems = Table.SelectedItems.Cast<RawLengthSet>().ToList();  // Replace YourItemType with the actual type.

            // Remove each selected item from the source collection
            foreach (var item in selectedItems)
            {
                items.Remove(item);
                JsonConverter.DeleteItemByDescription(item.Description);
            }
        }
        SettingsViewModel.Refresh();
    }

    private void Search_btn_click(object? sender, RoutedEventArgs e)
    {
        var input = this.FindControl<TextBox>("SearchBox")!.Text ?? "";
        input = input.ToUpper();
        RawStandardViewModel.LengthSetListForUI.Clear();
        foreach (var value in RawStandardViewModel.LengthSetList)
            if(value.Description.Contains(input!))
                RawStandardViewModel.LengthSetListForUI.Add(value);
        Console.WriteLine(RawStandardViewModel.LengthSetListForUI.Count);
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
    private void DataGrid_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
    {
        var cell = e.EditingElement as TextBox;
        if (cell != null)
        {
            // 포커스를 설정하고, 모든 텍스트를 선택하여 편집모드로 들어가게함
            cell.Focus();
            cell.SelectAll();
        }
    }
}