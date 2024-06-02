using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class ExcelTabView : TabView
{
    private readonly StartWindow mainWindow;
    private TableView tableView;
    private bool initialToggleState = true;
    
    public ExcelTabView(StartWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new ExcelTabViewModel();
    }
    
    private async void ReadExcelBtn_Click(object? sender, RoutedEventArgs e)
    {
        await mainWindow?.OpenSheetSelectWindow()!;
        tableView = new TableView();
        this.FindControl<Panel>("TablePanel")!.Children.Add(tableView);
    }

    private void DnDTaskBtn_Click(object? sender, RoutedEventArgs e)
    {
        WorkManager.ClassifyParts(); // Tbale view의 체크박스 상태에 따라 원본 리스트에서 작업용 리스트로 분리 
        mainWindow?.AddTab("파트 배치");
    }

    private void Toggle(object? sender, RoutedEventArgs e)
    {
        tableView.Toggle(initialToggleState);
        initialToggleState = !initialToggleState; // 다음 클릭 시 상태 반전
    }
}