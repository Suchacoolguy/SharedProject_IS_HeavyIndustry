using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Views.TabViews;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class StartWindow : Window
{
    public StartWindow()
    {
        InitializeComponent();
        //DataContext = new MainWindowViewModel();
    }
    //시트 선택창 띄우기
    public async Task OpenSheetSelectWindow()
    {
        var miniWindow = new SheetWindow(WorkManager.GetSheetNames(), this);
        await miniWindow.ShowDialog(this);
        if (!string.IsNullOrEmpty(WorkManager.SheetName))
            WorkManager.SetSheet(WorkManager.SheetName);
        
    }
    private async void NewProjectWindow_btn_click(object? sender, RoutedEventArgs e)
    {
        var newProjectWindow = new NewProjectWindow(this);
        await newProjectWindow.ShowDialog(this); // 새 프로젝트 생성창 열기
        
        if (string.IsNullOrEmpty(WorkManager.ExcelFilePath)) return;
        WorkManager.ReadExcelPackage();//파일 경로 확인 후 엑셀 읽기
        AddTab("프로젝트 정보"); // 탭 추가
    }

    //탭 패널에 드래그앤 드랍 탭 추가 
    private void AddDragNDrop_btn_click(object? sender, RoutedEventArgs e)
    {
        if (WorkManager.SheetName == null) return;
        WorkManager.ClassifyParts();
        AddTab("파트 배치");
    }

    private void Report_btn_click(object? sender, RoutedEventArgs e)
    {
        AddTab("레포트 출력");
    }
    
    public void AddTab(string tabHeader)
    {
        var tabItem = new TabItem
        {
            Header = tabHeader,
            Content = tabHeader switch
            {
                "프로젝트 정보" => new ExcelTabView(this),
                "파트 배치" => new DNDTabView(this),
                "레포트 출력" => new ReportTabView(),
                _ => throw new ArgumentOutOfRangeException()
            }
        };

        var tabPanel = this.FindControl<TabControl>("TabFrame");
        tabPanel?.Items.Add(tabItem);
    }
}