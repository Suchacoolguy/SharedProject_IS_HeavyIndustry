using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Services
{
    public class FilteringService
    {
        public FlyoutBase FilterMenu;
        private readonly string _tag;

        [Obsolete("Obsolete")]
        public FilteringService(string tag)
        {
            _tag = tag;
            FilterMenu = GenerateFilter();
        }
        
        [Obsolete("Obsolete")]
        private FlyoutBase GenerateFilter()
        {
            var contextFlyout = new Flyout
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = _tag,
                            FontWeight = FontWeight.Bold,
                            FontSize = 20,
                            Margin = new Thickness(5)
                        },
                        CreateFilterContent()
                    }
                }
            };

            return contextFlyout;
        }

        [Obsolete("Obsolete")]
        private Control CreateFilterContent()
        {
            var itemList = GetFilteringOptions();

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Height = 170,
                Width = 200
            };

            var panel = new StackPanel(){ Background = Brushes.White };

            // 검색 박스 추가
            var searchTextBox = new TextBox { Margin = new Thickness(0,3) };
            searchTextBox.TextChanged += (sender, args) => FilterCheckBoxes(panel, searchTextBox.Text!);

            var border = new Border()
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 2),
                Margin = new Thickness(0,0,0,5)
            };
            // "모두 선택" 체크박스 추가
            var selectAllCheckbox = new CheckBox
            {
                Content = "모두 선택",
                Margin = new Thickness(3, 0, 0, 5),
                IsChecked = true
            };
            selectAllCheckbox.Checked += SelectAllCheckbox_Checked!;
            selectAllCheckbox.Unchecked += SelectAllCheckbox_Unchecked!;

            border.Child = selectAllCheckbox;
            panel.Children.Add(border);

            // 나머지 아이템들 추가
            foreach (var item in itemList)
            {
                panel.Children.Add(new CheckBox() { Content = item, IsChecked = true, Margin = new Thickness(3,0)});
            }

            scrollViewer.Content = panel;

            var applyButton = new Button { Content = "적용", Tag = _tag, Margin = new Thickness(0,10,0,0), HorizontalAlignment = HorizontalAlignment.Right};
            applyButton.Click += FilterApply_Btn_Click;

            return new StackPanel
            {
                Children =
                {
                    searchTextBox,
                    scrollViewer,
                    applyButton
                }
            };
        }

        private List<string> GetFilteringOptions()
        {
            return _tag switch
            {
                "Description" => BOMDataViewModel.AllParts.Select(p => p.Desc.ToString()).Distinct().ToList(),
                "Assem" => BOMDataViewModel.AllParts.Select(p => p.Assem.ToString()).Distinct().ToList(),
                "Mark" => BOMDataViewModel.AllParts.Select(p => p.Mark.ToString()).Distinct().ToList(),
                "Material" => BOMDataViewModel.AllParts.Select(p => p.Material.ToString()).Distinct().ToList(),
                _ => new List<string>()
            };
        }

        private void SelectAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                // 체크된 경우, 모든 하위 체크박스를 체크 처리
                SetAllCheckBoxesChecked((checkBox.Parent!.Parent as Panel)!, true);
        }

        private void SelectAllCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                // 체크 해제된 경우, 모든 하위 체크박스를 체크 해제 처리
                SetAllCheckBoxesChecked((checkBox.Parent!.Parent as Panel)!, false);
        }

        private void SetAllCheckBoxesChecked(Panel panel, bool isChecked)
        {
            foreach (var child in panel.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0]) // 첫 번째는 "모두 선택" 체크박스이므로 제외
                    checkBox.IsChecked = isChecked;
        }

        private void FilterApply_Btn_Click(object? sender, RoutedEventArgs e)
        {
            // 적용 버튼 클릭 시 처리할 로직 작성
            if (sender is not Button applyButton) return;
            if (applyButton.Parent is not StackPanel stackPanel) return;
            var scrollViewer = stackPanel.Children.OfType<ScrollViewer>().FirstOrDefault();
            var panel = scrollViewer?.Content as StackPanel;

            var selectedItems = new List<string>();
            foreach (var child in panel!.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0] && checkBox.IsChecked == true)
                    selectedItems.Add(checkBox.Content!.ToString()!);

            BOMDataViewModel.ApplyFilter(applyButton.Tag?.ToString()!, selectedItems);
        }

        private void FilterCheckBoxes(Panel panel, string filter)
        {
            foreach (var child in panel.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0])
                    checkBox.IsVisible = string.IsNullOrEmpty(filter) || checkBox.Content!.ToString()!.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Services
{
    public class FilteringService
    {
        [Obsolete("Obsolete")]
        public static FlyoutBase GenerateFilter(string tag)
        {
            var contextFlyout = new Flyout
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = tag,
                            FontWeight = FontWeight.Bold,
                            FontSize = 20,
                            Margin = new Thickness(5)
                        },
                        CreateFilterContent(tag)
                    }
                }
            };

            return contextFlyout;
        }

        [Obsolete("Obsolete")]
        private static Control CreateFilterContent(string tag)
        {
            var itemList = GetFilteringOptions(tag);

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Height = 150,
                Width = 200
            };

            var panel = new StackPanel(){ Background = Brushes.White };

            // 검색 박스 추가
            var searchTextBox = new TextBox { Margin = new Thickness(0,3) };
            searchTextBox.TextChanged += (sender, args) => FilterCheckBoxes(panel, searchTextBox.Text!);
            
            // "모두 선택" 체크박스 추가
            var selectAllCheckbox = new CheckBox
            {
                Content = "모두 선택",
                Margin = new Thickness(0, 0, 0, 10),
                IsChecked = true
            };
            selectAllCheckbox.Checked += SelectAllCheckbox_Checked!;
            selectAllCheckbox.Unchecked += SelectAllCheckbox_Unchecked!;
            panel.Children.Add(selectAllCheckbox);

            // 나머지 아이템들 추가
            foreach (var item in itemList)
            {
                panel.Children.Add(new CheckBox() { Content = item, IsChecked = true });
            }

            scrollViewer.Content = panel;

            var applyButton = new Button { Content = "적용", Tag = tag, Margin = new Thickness(10) };
            applyButton.Click += FilterApply_Btn_Click;

            return new StackPanel
            {
                Children =
                {
                    searchTextBox,
                    scrollViewer,
                    applyButton
                }
            };
        }

        private static List<string> GetFilteringOptions(string type)
        {
            return type switch
            {
                "Description" => BOMDataViewModel.AllParts.Select(p => p.Desc.ToString()).Distinct().ToList(),
                "Assem" => BOMDataViewModel.AllParts.Select(p => p.Assem.ToString()).Distinct().ToList(),
                "Mark" => BOMDataViewModel.AllParts.Select(p => p.Mark.ToString()).Distinct().ToList(),
                "Material" => BOMDataViewModel.AllParts.Select(p => p.Material.ToString()).Distinct().ToList(),
                _ => new List<string>()
            };
        }

        private static void SelectAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                // 체크된 경우, 모든 하위 체크박스를 체크 처리
                SetAllCheckBoxesChecked((checkBox.Parent as Panel)!, true);
        }

        private static void SelectAllCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                // 체크 해제된 경우, 모든 하위 체크박스를 체크 해제 처리
                SetAllCheckBoxesChecked((checkBox.Parent as Panel)!, false);
        }

        private static void SetAllCheckBoxesChecked(Panel panel, bool isChecked)
        {
            foreach (var child in panel.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0]) // 첫 번째는 "모두 선택" 체크박스이므로 제외
                    checkBox.IsChecked = isChecked;
        }

        private static void FilterApply_Btn_Click(object? sender, RoutedEventArgs e)
        {
            // 적용 버튼 클릭 시 처리할 로직 작성
            if (sender is not Button applyButton) return;
            if (applyButton.Parent is not StackPanel stackPanel) return;
            var scrollViewer = stackPanel.Children.OfType<ScrollViewer>().FirstOrDefault();
            var panel = scrollViewer?.Content as StackPanel;

            var selectedItems = new List<string>();
            foreach (var child in panel!.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0] && checkBox.IsChecked == true)
                    selectedItems.Add(checkBox.Content!.ToString()!);

            BOMDataViewModel.ApplyFilter(applyButton.Tag?.ToString()!, selectedItems);
        }

        private static void FilterCheckBoxes(Panel panel, string filter)
        {
            foreach (var child in panel.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0])
                    checkBox.IsVisible = string.IsNullOrEmpty(filter) || checkBox.Content!.ToString()!.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
*/
