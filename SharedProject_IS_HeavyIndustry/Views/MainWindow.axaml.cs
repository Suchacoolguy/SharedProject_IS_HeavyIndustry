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
        private RotateTransform LoadingRotateTransform;
        private ScaleTransform LoadingScaleTransform;
        private DispatcherTimer _animationTimer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            /*LoadingRotateTransform = new RotateTransform();
            LoadingScaleTransform = new ScaleTransform();
            SetupLoadingAnimation();
            var loadingEllipse = this.FindControl<Ellipse>("LoadingEllipse");
            if (loadingEllipse != null)
            {
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(LoadingRotateTransform);
                transformGroup.Children.Add(LoadingScaleTransform);
                loadingEllipse.RenderTransform = transformGroup;
            }*/
        }

        public static List<string> GetSheetNames() // StartWindow에서 사용
        {
            return _workbook.Worksheets.Select(sh => sh.Name).ToList();
        }

        public static void SetSheet(string sheetName) // StartWindow에서 사용
        {
            if (string.IsNullOrEmpty(sheetName)) return;
            _sheet = _workbook.Worksheet(sheetName);

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

        public async Task ReadExcelWorkbookAsync()
        {
            this.FindControl<Grid>("LoadingGrid")!.IsVisible = true; // 로딩 표시 보이기
            //StartLoadingAnimation();
            await Task.Run(() => _workbook = ExcelDataReader.Read(ExcelTabViewModel.ExcelFilePath)); // 비동기 작업
            //StopLoadingAnimation();
            this.FindControl<Grid>("LoadingGrid")!.IsVisible = false; // 로딩 표시 숨기기
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
        }

        private Control CreateTabContent(string tabHeader)
        {
            
            return tabHeader switch
            {
                "프로젝트 정보" => new BOMDataTabView(this),
                "파트 배치" => new DragAndDropTabView(this),
                "레포트 출력" => new ReportTabView(),
                "규격 목록" => new RawStandardTabView(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /*private void SetupLoadingAnimation()
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // 약 60 FPS
            };
            _animationTimer.Tick += (sender, e) =>
            {
                LoadingRotateTransform.Angle += 6;
                if (LoadingRotateTransform.Angle >= 360)
                {
                    LoadingRotateTransform.Angle = 0;
                }

                LoadingScaleTransform.ScaleX = 1.0 + 0.1 * Math.Sin(LoadingRotateTransform.Angle * Math.PI / 180);
                LoadingScaleTransform.ScaleY = 1.0 + 0.1 * Math.Sin(LoadingRotateTransform.Angle * Math.PI / 180);
            };
        }*/

        /*private void StartLoadingAnimation()
        {
            _animationTimer.Start();
        }

        private void StopLoadingAnimation()
        {
            _animationTimer.Stop();
        }*/
    }
}
