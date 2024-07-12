using System;
using System.Text.RegularExpressions;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;
using System.IO;

namespace SharedProject_IS_HeavyIndustry.Views
{
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
            WriteImageSizeFile();
            if (MainWindowViewModel.RawMaterialSet.Count < 1)
            {
                MessageService.Send("작업된 항목이 없습니다");
                return;
            }
            ExcelDataWriter.Write(MainWindowViewModel.RawMaterialSet);
        }

        private void WriteImageSizeFile()
        {
            var viewModel = DataContext as ReportTabViewModel;
            if (viewModel != null)
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var filePath = Path.Combine(appDirectory, "Assets", "imageSize.txt");

                if (!Directory.Exists(Path.Combine(appDirectory, "Assets")))
                {
                    Directory.CreateDirectory(Path.Combine(appDirectory, "Assets"));
                }

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"Width={ReportTabViewModel.Width}");
                    writer.WriteLine($"Height={ReportTabViewModel.Height}");
                }
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); // 정수만 허용
            return !regex.IsMatch(text);
        }
    }
}
