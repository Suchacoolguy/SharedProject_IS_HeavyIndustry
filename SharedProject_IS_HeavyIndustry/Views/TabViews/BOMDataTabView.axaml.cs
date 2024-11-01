﻿using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Office.CustomUI;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;
using Avalonia.Controls.Primitives;
using OperationsResearch;
using Button = Avalonia.Controls.Button;
using ToggleButton = Avalonia.Controls.Primitives.ToggleButton;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class BOMDataTabView : TabView
{
    private readonly MainWindow mainWindow;
    private static TableView? tableView;
    private bool initialToggleState = true;
    private static ToggleButton? exclude;
    private static ToggleButton? separate;
    private static ToggleButton? needSeparate;

    public BOMDataTabView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new ExcelTabViewModel();
    }

    public static void RefreshTableView()
    {
        MainWindow.SetSheet(BOMDataViewModel.SheetName);
        tableView = new TableView();
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

        if (!SeparateLengthValidCheck()) return;

        if (MainWindowViewModel.RawMaterialSet.Count > 0)
            MainWindowViewModel.RawMaterialSet.Clear();
        if (MainWindowViewModel.TempPartSet.Count > 0)
            MainWindowViewModel.TempPartSet.Clear();
        // if ()
        // Console.WriteLine("-------------------" + MainWindowViewModel.DragAndDropViewModel.TempPartList.Count());

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
        tableView?.SetExclude(true);
    }
    
    private void SetExcludeFalse(object? sender, RoutedEventArgs e)
    {
        tableView?.SetExclude(false);
    }

    private void SetSeparateTrue(object? sender, RoutedEventArgs e)
    {
        tableView?.SetSeparate(true);
    }
    
    private void SetSeparateFalse(object? sender, RoutedEventArgs e)
    {
        tableView?.SetSeparate(false);
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
        var length = inputBox.Text;
        
        if(InputLengthValidCheck(length!))
            foreach (var part in BOMDataViewModel.PartsFiltered)
                part.lengthToBeSeparated = length!;
        else
        {
            MessageService.Send("유효하지 않은 값입니다 ");
        }

        inputBox.Text = "";

    }

    private bool InputLengthValidCheck(string length)
    {
        // 쉼표로 구분된 두 개의 숫자를 처리
        var lengths = length.Split(',');

        // 두 개의 숫자를 확인
        if (lengths.Length == 2)
        {
            if (int.TryParse(lengths[0], out var firstNumber) && firstNumber > 0 &&
                int.TryParse(lengths[1], out var secondNumber) && secondNumber > 0)
                return true;
        }
        // 쉼표가 없을 경우, 하나의 숫자만 확인
        else if (int.TryParse(length, out var singleNumber) && singleNumber > 0)
            return true;
        
        return false;
    }

    private bool SeparateLengthValidCheck()
    {
        HashSet<string> descSet = [];
        List<string> descList = [];
        foreach (var part in BOMDataViewModel.AllParts.Where(p => p.NeedSeparate))
        {
            if (string.IsNullOrEmpty(part.lengthToBeSeparated))
            {
                if (descSet.Add(part.Desc.ToString()))
                    descList.Add(part.Desc.ToString());
            }
            else
            {
                List<int> list;
                try
                {
                    list = part.GetSeperateLengthList();
                }catch (Exception e)
                {
                    descList.Add(part.Desc.ToString());
                    continue;
                }

                foreach (var len in list)
                {
                    if (SettingsViewModel.GetMaxLen(part.Desc.ToString()) >= len) continue;
                    if (descSet.Add(part.Desc.ToString()))
                        descList.Add(part.Desc.ToString());
                }
            }
        }

        if (descList.Count <= 0) return true;
        var msg = descList.Aggregate("", (current, desc) => current + (desc + "\n")) + "위 항목의 분리 길이를 다시 설정해주세요";
        MessageService.Send(msg);
        return false;

    }
}