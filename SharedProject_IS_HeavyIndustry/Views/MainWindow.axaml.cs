using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ClosedXML.Excel;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;
using SharedProject_IS_HeavyIndustry.Views.TabViews;

namespace SharedProject_IS_HeavyIndustry.Views
{
    public partial class MainWindow : Window
    {
        private static IXLWorksheet _sheet = null!;
        private static XLWorkbook _workbook = null!;
        private Loading loading;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            loading = this.FindControl<Loading>("LoadingControl")!;
        }

        public static List<string> GetSheetNames() // StartWindow에서 사용
        {
            return _workbook.Worksheets.Select(sh => sh.Name).ToList();
        }

        public static void SetSheet(string sheetName) // StartWindow에서 사용
        {
            if (string.IsNullOrEmpty(sheetName)) return;
            _sheet = _workbook.Worksheet(sheetName);
            
            SettingsViewModel.Refresh();

            //시트를 선택하면 시트의 part정보를 뽑아옴
            try
            {
                List<Part> partsFromBOM = ExcelDataReader.PartListFromExcel(_sheet);
                MainWindowViewModel.BomDataViewModel = new BOMDataViewModel(partsFromBOM);    
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle the exception
                Console.WriteLine("Error: " + ex.Message);
            }
            catch (IOException ex)
            {
                // Handle other I/O exceptions
                Console.WriteLine("Error: " + ex.Message);
            }
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
            await ReadExcelWorkbookAsync(); //파일 경로 확인 후 엑셀 읽기
            AddTab("프로젝트 정보"); // 탭 추가
        }
        
        public async Task ReadExcelWorkbookAsync()
        {
            loading.Start(); // 로딩 시작
            await Task.Run(() => _workbook = ExcelDataReader.Read(ExcelTabViewModel.ExcelFilePath)); // 비동기 작업
            loading.Stop(); // 로딩 종료
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
        
        private void HyungGang_btn_click(object? sender, RoutedEventArgs e)
        {
            AddTab("형강 목록");
        }

        public void AddTab(string tabHeader)
        {
            var tabControl = this.FindControl<TabControl>("TabFrame");
            var existingTab = tabControl?.Items.Cast<TabItem>().FirstOrDefault(item => item.Header!.ToString() == tabHeader);

            if (existingTab != null)
            {
                existingTab.Content = CreateTabContent(tabHeader);
                tabControl!.SelectedItem = existingTab; // 기존 탭으로 이동
            }
            else
            {
                var tabItem = new TabItem
                {
                    Header = tabHeader,
                    Content = CreateTabContent(tabHeader)
                };

                tabControl?.Items.Add(tabItem);
                tabControl!.SelectedItem = tabItem; // 새로 추가된 탭으로 이동
            }
        }

        /*public void AddTab(string tabHeader)
        {
            var tabPanel = this.FindControl<TabControl>("TabFrame");
            var existingTab = tabPanel?.Items.Cast<TabItem>().FirstOrDefault(item => item.Header!.ToString() == tabHeader);

            if (existingTab != null)
            {
                existingTab.Content = CreateTabContent(tabHeader);
            }
            else
            {
                var tabItem = new TabItem
                {
                    Header = tabHeader,
                    Content = CreateTabContent(tabHeader)
                };

                tabPanel?.Items.Add(tabItem);
            }
        }*/

        private Control CreateTabContent(string tabHeader)
        {
            
            return tabHeader switch
            {
                "프로젝트 정보" => new BOMDataTabView(this),
                "파트 배치" => new DragAndDropTabView(this),
                "레포트 출력" => new ReportTabView(),
                "규격 목록" => new RawStandardTabView(),
                "형강 목록" => new HyungGangTabView(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void Bell_Click_Event(object? sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (!btn!.Name!.Equals("BellActive")) return;
            btn.IsVisible = false;
            this.FindControl<Button>("Bell")!.IsVisible = true;
        }
    }
}
