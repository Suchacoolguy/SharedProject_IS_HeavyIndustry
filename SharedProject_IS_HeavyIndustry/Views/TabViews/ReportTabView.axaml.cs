using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class ReportTabView : TabView
{
    public ReportTabView()
    {
        DataContext = new ReportTabViewModel();
        InitializeComponent();
    }
    private async void PlanReview_btn_click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard("Caption", "Are you sure you would like to delete appender_replace_page_1?",
                ButtonEnum.Ok);

        var result = await box.ShowAsync();
        // throw new System.NotImplementedException();
    }
    private void PrintPlan_btn_click(object? sender, RoutedEventArgs e)
    {
        ExcelDataWriter.Write(MainWindowViewModel.RawMaterialSet);
    }
    
}