using Avalonia.Interactivity;
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
    private void PlanReview_btn_click(object? sender, RoutedEventArgs e)
    {
        // throw new System.NotImplementedException();
    }
    private void PrintPlan_btn_click(object? sender, RoutedEventArgs e)
    {
        ExcelDataWriter.Write(MainWindowViewModel.RawMaterialSet);
    }
    
}