﻿using System;
using Avalonia.Interactivity;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class NewProjectWindow : Window
{
    private Window parentWindow;
    private string? filePath;
    public NewProjectWindow(Window parent)
    {
        parentWindow = parent;
        InitializeComponent();
    }
    
    private async void OpenDirectory(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 파일 탐색기 열기
        var dialog = new OpenFileDialog
        {
            Title = "엑셀 파일 선택",
            Filters =
            [
                new FileDialogFilter { Name = "Excel Files", Extensions = ["xlsx", "xls"] }
            ]
        };
        var result = await dialog.ShowAsync(this);

        if (result != null) 
            filePath = result.FirstOrDefault();

        //텍스트 박스에 선택한 파일 경로 출력
        this.FindControl<TextBox>("FilePathBox")!.Text = filePath;
    }
    
    private bool IsExcelFile()
    {
        // 파일 확장자 확인
        var extension = Path.GetExtension(filePath);
        return extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    private void Confirm(object? sender, RoutedEventArgs e)
    {
        // 파일이 엑셀 파일인지 확인
        if (!string.IsNullOrEmpty(filePath) && IsExcelFile())
        {
            ExcelTabViewModel.ExcelFilePath = filePath;
            var projectName = this.FindControl<TextBox>("ProjectName")?.Text;
            if (projectName != null)
                MainWindowViewModel.ProjectName = projectName;
            Close();
        }
        else
        {
            Close();
        }
    }

    private void Cancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}