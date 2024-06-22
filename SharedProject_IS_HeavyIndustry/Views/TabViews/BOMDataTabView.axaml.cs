using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class BOMDataTabView : TabView
{
    private readonly StartWindow mainWindow;
    private TableView tableView;
    private bool initialToggleState = true;
    
    public BOMDataTabView(StartWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new ExcelTabViewModel();
    }
    
    private async void ReadExcelBtn_Click(object? sender, RoutedEventArgs e)
    {
        await mainWindow?.OpenSheetSelectWindow()!;
        tableView = new TableView();
        var tablePanel = this.FindControl<Panel>("TablePanel")!;
        tablePanel.Children.Clear(); // Clear existing children
        tablePanel.Children.Add(tableView);
    }

    private void DnDTaskBtn_Click(object? sender, RoutedEventArgs e)
    {
        BOMDataViewModel.ClassifyParts(); // Tbale view의 체크박스 상태에 따라 원본 리스트에서 작업용 리스트로 분리 
        mainWindow?.AddTab("파트 배치");
    }

    private void Toggle(object? sender, RoutedEventArgs e)
    {
        tableView.ToggleNeedSeperate(true);
        initialToggleState = !initialToggleState; // 다음 클릭 시 상태 반전
    }
    
    private void SetExcluded(object? sender, RoutedEventArgs e)
    {
        tableView.SetExcludeTrue();
    }
    
    private void SetNotExcluded(object? sender, RoutedEventArgs e)
    {
        tableView.SetExcludeFalse();
        initialToggleState = !initialToggleState; // 다음 클릭 시 상태 반전
    }

    private void MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        if (menuItem != null)
        {
            // Do something with menuItem.Header
            Console.WriteLine($"You clicked {menuItem.Header}");
        }
    }
}