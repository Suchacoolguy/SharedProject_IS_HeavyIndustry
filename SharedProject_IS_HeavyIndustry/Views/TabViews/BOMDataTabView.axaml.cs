using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Office.CustomUI;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;
using Avalonia.Controls.Primitives;
using Button = Avalonia.Controls.Button;
using ToggleButton = Avalonia.Controls.Primitives.ToggleButton;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class BOMDataTabView : TabView
{
    private readonly MainWindow mainWindow;
    private TableView tableView;
    private bool initialToggleState = true;
    private static ToggleButton exclude, separate, needSeparate;
    
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
        FilteringService.Clear();//시트가 바뀌면 필터 또한 클리어 
        tablePanel.Children.Clear(); // Clear existing children
        tablePanel.Children.Add(tableView);
        if (RawStandardViewModel.MissingStandardBuffer.Count > 0)
        {
            mainWindow.AddTab("규격 목록");
            RawStandardViewModel.AddMissingData();
        }
    }

    private async void DnDTaskBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (!CheckSeparateCondition())
        {
            var answer = await MessageService.SendWithAnswer("분리 필요한 파트가 \n분리 설정 되지 않았습니다.\n계속 작업 하시겠습니까?");
            if(!answer)
                return;
        }
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

    private bool CheckSeparateCondition()
    {
        foreach (var p in BOMDataViewModel.AllParts)
        {
            if (!p.IsOverLenth == p.NeedSeparate)
                return false;
        }
        return true;
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
    
    private void AddToggleFilter(object? sender, RoutedEventArgs e)
    {
        var toggleOption = sender as ToggleButton;
        switch (toggleOption!.Tag!.ToString()!)
        {
            case "ExcludeCheck":
                BOMDataViewModel.ExcludeCheck = toggleOption.IsChecked == true;
                exclude ??= toggleOption;
                break;
            case "SeparateCheck":
                BOMDataViewModel.SeparateCheck = toggleOption.IsChecked == true;
                separate ??= toggleOption;
                break;
            case "NeedSeparateCheck":
                BOMDataViewModel.NeedSeparateCheck = toggleOption.IsChecked == true;
                needSeparate ??= toggleOption;
                break;
        }
        BOMDataViewModel.ApplyToggleFilter();
    }
    
    private void ReleaseFilter_Btn_Click(object? sender, RoutedEventArgs e)
    {
        BOMDataViewModel.ReleaseAllFilter();
    }

    //토글버튼 모두 끔
    public static void OffSwitches()
    {
        if (exclude != null)
            exclude.IsChecked = false;
        if (separate != null)
            separate.IsChecked = false;
        if (needSeparate != null)
            needSeparate.IsChecked = false;
    }

    private void Input_Btn_Clicked(object? sender, RoutedEventArgs e)
    {
        var inputBox = this.FindControl<TextBox>("SeparateLenBox")!;
        foreach (var part in BOMDataViewModel.PartsFiltered)
        {
            part.lengthToBeSeparated = inputBox.Text!;
        }

        inputBox.Text = "";

    }
}