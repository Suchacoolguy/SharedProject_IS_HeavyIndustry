using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class BOMDataTabView : TabView
{
    private readonly MainWindow mainWindow;
    private TableView tableView;
    private bool initialToggleState = true;
    
    public BOMDataTabView(MainWindow mainWindow)
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
        if (MainWindowViewModel.BomDataViewModel == null)
        {
            MessageService.Send("시트를 선택해 주세요");
            return;
        }
        if(MainWindowViewModel.RawMaterialSet.Count != 0)
            MainWindowViewModel.RawMaterialSet.Clear();
        BOMDataViewModel.ClassifyParts(); // Tbale view의 체크박스 상태에 따라 원본 리스트에서 작업용 리스트로 분리 
        mainWindow?.AddTab("파트 배치");
    }
    
    private void SetExcludeTrue(object? sender, RoutedEventArgs e)
    {
        tableView.SetExclude(true);
    }
    
    private void SetExcludeFalse(object? sender, RoutedEventArgs e)
    {
        tableView.SetExclude(false);
    }

    private void SetSeparateTrue(object? sender, RoutedEventArgs e)
    {
        tableView.SetSeparate(true);
    }
    
    private void SetSeparateFalse(object? sender, RoutedEventArgs e)
    {
        tableView.SetSeparate(false);
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