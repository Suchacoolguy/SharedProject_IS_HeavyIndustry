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
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
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

        private void PrintPlan_btn_click(object? sender, RoutedEventArgs e)
        {
            // those that have not been worked on (not arranged).
            List<string> emptyKeys = FindEmptyKeys();
            // MyProgressBar.Maximum = emptyKeys.Count;

            if (MainWindowViewModel.RawMaterialSet.Count < 1)
            {
                MessageService.Send("최소한 하나의 규격에 대해 파트 배치를 진행한 뒤 진행해주세요.");
                return;
            }

            // Perform the arrangement on the empty keys here.
            // await Task.Run(() =>
            // {
                for (int i = 0; i < emptyKeys.Count; i++)
                {
                    string key = emptyKeys[i];
                    
                    // Perform arrangement for each part
                    ArrangePartsForEmptyKey(key);

                    // Update the progress bar on the UI thread
                    // Dispatcher.UIThread.InvokeAsync(() =>
                    // {
                    //     MyProgressBar.Value = i + 1;
                    // });
                }
            // });

            // After processing all keys, execute the following on the UI thread
            // Dispatcher.UIThread.InvokeAsync(() =>
            // {
                WriteImageSizeFile();
                ExcelDataWriter.Write(MainWindowViewModel.RawMaterialSet);
            // });
        }

        
        private void ArrangePartsForEmptyKey(string key)
        {
            var parts = GetFilteredPartsForKey(key, BOMDataViewModel.PartsForTask);
            var partsOverLength = GetFilteredPartsForKey(key, BOMDataViewModel.PartsToSeparate);

            // Get the length options from settings
            var lengthOptions = SettingsViewModel.GetLengthOption(key.Split(',')[1]);
            int maxLen = lengthOptions.Max();

            // Create the ArrangePartsService instance
            var service = new ArrangePartsService(new List<Part>(parts), partsOverLength, lengthOptions);
            var temp_res = service.GetArrangedRawMaterials();
            var temp_res_overSize = service.getPartsCanNotBeArranged();

            List<RawMaterial> res = temp_res.ToList();
            
            // 원자재 길이 오름차순으로 정렬하고, 원자재 길이가 같은 것들은 잔량 기준으로 내림차순 정렬
            res = res.OrderBy(p => p.Length).ThenByDescending(p => p.RemainingLength).ToList();
                    
            // 리스트를 ObservableCollection로 변환
            ObservableCollection<RawMaterial> arrangedPartsCollectionSorted = new ObservableCollection<RawMaterial>(res);
            
            // Update the RawMaterialSet and TempPartSet
            if (!MainWindowViewModel.RawMaterialSet.ContainsKey(key))
            {
                MainWindowViewModel.RawMaterialSet.TryAdd(key, arrangedPartsCollectionSorted);
            }
                    
            // 파트배치 불가능했던 애들도 있으면 넣어주자~
            List<Part> res_overSize = temp_res_overSize.ToList();
            res_overSize = res_overSize.OrderBy(p => p.Length).ToList();
            ObservableCollection<Part> overSizeCollectionSorted = new ObservableCollection<Part>(res_overSize);
            
            if (!MainWindowViewModel.TempPartSet.ContainsKey(key))
            {
                MainWindowViewModel.TempPartSet.TryAdd(key, overSizeCollectionSorted);
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
