using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using OfficeOpenXml;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;
using SharedProject_IS_HeavyIndustry.Views.TabViews;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class StartWindow : Window
{
    private static ExcelWorksheet _sheet = null!;
    private static ExcelPackage _package = null!;
    
    public StartWindow()
    {
        InitializeComponent();
        //DataContext = new MainWindowViewModel();
    }
    
    public static List<string> GetSheetNames() // StartWindow에서 사용
    {
        return _package.Workbook.Worksheets.Select(sh => sh.Name).ToList();
    }
    
    public static void SetSheet(string sheetName) // StartWindow에서 사용
    {
        if (string.IsNullOrEmpty(sheetName)) return;
        _sheet = _package.Workbook.Worksheets[sheetName];
        //시트를 선택하면 시트의 part정보를 뽑아옴
        List<Part> partsFromBOM = ExcelDataReader.PartListFromExcel(_sheet);
        MainWindowViewModel.BomDataViewModel = new BOMDataViewModel(partsFromBOM);
    }

    public static void ReadExcelPackage() // StartWindow에서 사용
    {
        _package = ExcelDataReader.Read(ExcelTabViewModel.ExcelFilePath);
    }
    
    
    //시트 선택창 띄우기
    public async Task OpenSheetSelectWindow()
    {
        var miniWindow = new SheetSelectionWindow(GetSheetNames(), this);
        await miniWindow.ShowDialog(this);
        if (!string.IsNullOrEmpty(BOMDataViewModel.SheetName))
            SetSheet(BOMDataViewModel.SheetName);
    }
    
    //새 프로젝트 생성창 띄우기
    private async void NewProjectWindow_btn_click(object? sender, RoutedEventArgs e)
    {
        var newProjectWindow = new NewProjectWindow(this);
        await newProjectWindow.ShowDialog(this); // 새 프로젝트 생성창 열기
        
        if (string.IsNullOrEmpty(ExcelTabViewModel.ExcelFilePath)) return;
        ReadExcelPackage();//파일 경로 확인 후 엑셀 읽기
        AddTab("프로젝트 정보"); // 탭 추가
    }

    //탭 패널에 드래그앤 드랍 탭 추가 
    private void AddDragNDrop_btn_click(object? sender, RoutedEventArgs e)
    {
        if (BOMDataViewModel.SheetName == null) return;
        BOMDataViewModel.ClassifyParts();
        AddTab("파트 배치");
    }

    private void Report_btn_click(object? sender, RoutedEventArgs e)
    {
        AddTab("레포트 출력");
    }
    
    private void Standard_btn_click(object? sender, RoutedEventArgs e)
    {
        AddTab("규격 목록");
    }
    
    public void AddTab(string tabHeader)
    {
        var tabItem = new TabItem
        {
            Header = tabHeader,
            Content = tabHeader switch
            {
                "프로젝트 정보" => new BOMDataTabView(this),
                "파트 배치" => new DragAndDropTabView(this),
                "레포트 출력" => new ReportTabView(),
                "규격 목록" => new RawStandardTabView(),
                _ => throw new ArgumentOutOfRangeException()
            }
        };

        var tabPanel = this.FindControl<TabControl>("TabFrame");
        tabPanel?.Items.Add(tabItem);
    }
}