using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Threading.Tasks;
using Avalonia.Threading;
using SharedProject_IS_HeavyIndustry.Views.TabViews;

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
        
        private static ObservableCollection<Part> GetFilteredParts(string material, string desc, ObservableCollection<Part> parts)
        {
            var list = new ObservableCollection<Part>();

            foreach (var part in parts)
                if (part.Desc.ToString().Equals(desc) && part.Material.Equals(material))
                    list.Add(part);

            return list;
        }
        
        private Dictionary<string, List<string>> GetFilterSet()
        {
            var dictionary = new Dictionary<string, List<string>>();
            GetFilterSetFromParts(dictionary, BOMDataViewModel.AllParts);
            return dictionary;
        }

        private void GetFilterSetFromParts(Dictionary<string, List<string>> dictionary, ObservableCollection<Part> parts)
        {
            foreach (var part in parts)
            {
                if (part.IsExcluded) continue;

                // Combine material and description to match the MainWindowViewModel.SelectedKey format
                string combinedKey = part.Material + "," + part.Desc.ToString();

                if (!dictionary.ContainsKey(combinedKey))
                {
                    dictionary.Add(combinedKey, new List<string> { combinedKey });
                }
            }
        }
        
        public List<string> FindEmptyKeys()
        {
            Dictionary<string, List<string>> filterSet = GetFilterSet();
        
            List<string> emptyKeys = new List<string>();

            foreach (var entry in filterSet)
            {
                foreach (var description in entry.Value)
                {
                    if (!MainWindowViewModel.RawMaterialSet.TryGetValue(description, out var rawMaterials))
                    {
                        if (rawMaterials == null || rawMaterials.Count == 0)
                            emptyKeys.Add(description);  // 비어있다면 키를 리스트에 추가합니다.
                    }
                }
            }

            return emptyKeys;  // 비어있는 키들의 리스트를 반환합니다.
        }

        private async void PrintPlan_btn_click(object? sender, RoutedEventArgs e)
        {
            // those that have not been worked on (not arranged).
            List<string> emptyKeys = FindEmptyKeys();
            MyProgressBar.Maximum = emptyKeys.Count;

            if (MainWindowViewModel.RawMaterialSet.Count < 1)
            {
                MessageService.Send("작업된 항목이 없습니다");
                return;
            }

            // Perform the arrangement on the empty keys here.
            await Task.Run(() =>
            {
                for (int i = 0; i < emptyKeys.Count; i++)
                {
                    string key = emptyKeys[i];
                    
                    // Perform arrangement for each part
                    ArrangePartsForEmptyKey(key);

                    // Update the progress bar on the UI thread
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        MyProgressBar.Value = i + 1;
                    });
                }
            });

            // After processing all keys, execute the following on the UI thread
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                WriteImageSizeFile();
                ExcelDataWriter.Write(MainWindowViewModel.RawMaterialSet);
            });
        }

        
        private void ArrangePartsForEmptyKey(string key)
        {
            var parts = GetFilteredPartsForKey(key, BOMDataViewModel.PartsForTask);
            var partsOverLength = GetFilteredPartsForKey(key, BOMDataViewModel.PartsToSeparate);

            // Get the length options from settings
            var lengthOptions = SettingsViewModel.GetLengthOption(key.Split(',')[1]);

            // Create the ArrangePartsService instance
            var service = new ArrangePartsService(new List<Part>(parts), partsOverLength, lengthOptions);
            var res = service.GetArrangedRawMaterials();

            // Update the RawMaterialSet and TempPartSet
            if (!MainWindowViewModel.RawMaterialSet.ContainsKey(key))
            {
                MainWindowViewModel.RawMaterialSet.TryAdd(key, res);
            }
        }
        
        private ObservableCollection<Part> GetFilteredPartsForKey(string key, ObservableCollection<Part> parts)
        {
            var partsList = new ObservableCollection<Part>();
            var keyParts = key.Split(',');
            var material = keyParts[0];
            var desc = keyParts[1];

            foreach (var part in parts)
            {
                if (part.Material.Equals(material) && part.Desc.ToString().Equals(desc))
                {
                    partsList.Add(part);
                }
            }

            return partsList;
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

                string isVisibleLine = $"IsVisible={ReportTabViewModel.IsVisible}";
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("IsVisible="))
                        {
                            isVisibleLine = line;
                            break;
                        }
                    }
                }

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"Width={ReportTabViewModel.Width}");
                    writer.WriteLine($"Height={ReportTabViewModel.Height}");
                    writer.WriteLine(isVisibleLine);
                }
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); // 정수만 허용
            return !regex.IsMatch(text);
        }

        private void MakeInvisible(object? sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ReportTabViewModel;
            if (viewModel != null)
            {
                ReportTabViewModel.IsVisible = false;

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
                    writer.WriteLine($"IsVisible={ReportTabViewModel.IsVisible}");
                }
            }
        }
    }
}
