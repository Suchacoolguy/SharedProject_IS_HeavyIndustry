using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Views.TabViews;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class StartWindow : Window
{
    private string filePath;
    private ExcelManager exm;
    private List<Part> parts;
    public StartWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
    
    private async void OpenDirectory(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 파일 탐색기를 엽니다.
        var dialog = new OpenFileDialog();
        var result = await dialog.ShowAsync(this);

        filePath = result.FirstOrDefault();
        // 파일이 엑셀 파일인지 확인
        if (!IsExcelFile(filePath) && filePath != null)
            return;
        
        exm = new ExcelManager(filePath);
        AddTab("프로젝트 정보");
    }
    
    private static DataGridTextColumn CreateColumn(string header, string bindingPath)
    {
        return new DataGridTextColumn
        {
            Header = header,
            Binding = new Binding(bindingPath),
            Width = DataGridLength.SizeToCells
        };
    }
    
    private bool IsExcelFile(string filePath)
    {
        // 파일 확장자 확인
        string extension = Path.GetExtension(filePath);
        return extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
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
                "sibal" => new DragAndDropView(this),
                _ => throw new ArgumentOutOfRangeException()
            }
        };

        var tabPanel = this.FindControl<TabControl>("TabFrame");
        tabPanel?.Items.Add(tabItem);
    }

    //시트 선택창 띄우기
    public async void OpenSheetSelectWindow()
    {
        var miniWindow = new SheetWindow(exm.GetSheetNames(), this);
        await miniWindow.ShowDialog(this);
        if (!string.IsNullOrEmpty(WorkManager.SheetName))
            WorkManager.Parts = exm.GetPartsFromSheet(WorkManager.SheetName);
        Console.WriteLine("WorkManager : " + WorkManager.SheetName);
    }
    /*var result = await miniWindow.ShowDialog<bool?>(this);*/

    //탭 패널에 드래그앤 드랍 탭 추가 
    private void AddDragNDrop(object? sender, RoutedEventArgs e)
    {
        AddTab("파트 배치");
    }

    private void sibal(object? sender, RoutedEventArgs e)
    {
        AddTab("sibal");
    }
}