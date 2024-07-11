using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
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
        MessageService.Send("아직 구현되지 않은 페이지 입니다");
    }
    private void PrintPlan_btn_click(object? sender, RoutedEventArgs e)
    {
        if (MainWindowViewModel.RawMaterialSet.Count < 1)
        {
            MessageService.Send("작업된 항목이 없습니다");
            return;
        }
        ExcelDataWriter.Write(MainWindowViewModel.RawMaterialSet);
    }
    
}