using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class ExcelTabView : TabView
{
    private StartWindow mainWindow;
    public ExcelTabView()
    {
        InitializeComponent();
        DataContext = new ExcelTabViewModel();
    }
    
    public ExcelTabView(StartWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new ExcelTabViewModel();
    }
    
    private async void ReadExcelBtn_Click(object? sender, RoutedEventArgs e)
    {
        await mainWindow?.OpenSheetSelectWindow()!;
        this.FindControl<Panel>("TablePanel")!.Children.Add(new TableView());
    }

    private void DnDTaskBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainWindow?.AddTab("파트 배치");
    }
}