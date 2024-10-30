using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using DocumentFormat.OpenXml.Drawing.Charts;
using SharedProject_IS_HeavyIndustry.ViewModels;
using Thickness = Avalonia.Thickness;

namespace SharedProject_IS_HeavyIndustry.Services
{
    public class FilteringService
    {
        private static string? _key;
        private static OrderedDictionary filterSet = new();
        public static List<string> SelectedItems = [];

        [Obsolete("Obsolete")]
        public static FlyoutBase? GetFilterMenu(string? tag) // filterStack에 필터 존재 여부 확인 후 반환, 없으면 추가후 반환
        {

            //필터셋에 이미 필터가 존재하는데 필터의 상태가 모두 선택이면 필터 없는것과 같으므로 해당 필터를 셋에서 제외
            if (_key != null && filterSet.Count > 0 && filterSet.Contains(_key))
            {
                if (GetAllSelectCheckState(((FlyoutBase)filterSet[_key]!)))
                    filterSet.RemoveAt(filterSet.Count - 1);
            }
            _key = tag;
            FlyoutBase? filter = null;
            
            if (tag != null && filterSet.Contains(tag))
                return ((FlyoutBase?)filterSet[tag])!;
            filter = GenerateFilter();
            if (tag != null) 
                filterSet.Add(tag, filter);
            return filter;
        }

        public static void Clear()
        {
            _key = "";
            filterSet.Clear();
            SelectedItems = [];
        }
        
        [Obsolete("Obsolete")]
        private static FlyoutBase? GenerateFilter()
        {
            var contextFlyout = new Flyout
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = _key,
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
        private static Control CreateFilterContent()
        {
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
            searchTextBox.TextChanged += (sender, args) => SelectCheckBoxes(panel, searchTextBox.Text!);

            var border = new Border()
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 2),
                Margin = new Thickness(0,0,0,5)
            };
            // "모두 선택" 체크박스 추가
            var selectAllCheckbox = new CheckBox
            {
                IsThreeState = true,
                Content = "모두 선택",
                Margin = new Thickness(3, 0, 0, 5),
                IsChecked = true
            };
            selectAllCheckbox.Checked += SelectAllCheckbox_Checked!;
            selectAllCheckbox.Unchecked += SelectAllCheckbox_Unchecked!;
            
            border.Child = selectAllCheckbox;
            panel.Children.Add(border);

            // 나머지 아이템들 추가
            foreach (var item in GetFilteringOptions())
            {
                var checkBox = new CheckBox() { Content = item, IsChecked = true, Margin = new Thickness(3,0)};
                checkBox.Checked += CheckBox_StateChanged;
                checkBox.Unchecked += CheckBox_StateChanged;
                panel.Children.Add(checkBox);
            }

            scrollViewer.Content = panel;

            var applyButton = new Button { Content = "적용", Tag = _key, Margin = new Thickness(0,10,0,0), HorizontalAlignment = HorizontalAlignment.Right};
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

        
        private static void CheckBox_StateChanged(object? sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;
            if (checkBox.Parent is not StackPanel panel) return;
            var allCheckBox = panel.Children.OfType<Border>().FirstOrDefault()!.Child as CheckBox;
            if(checkBox.IsChecked == false && allCheckBox!.IsChecked == true)
                allCheckBox!.IsChecked = null;
            else
            {
                var allChecked = panel.Children.OfType<CheckBox>().Where(cb => cb != allCheckBox).All(cb => cb.IsChecked == true);
                if (allChecked)
                    allCheckBox!.IsChecked = true;
            }
        }

        private static List<string> GetFilteringOptions()
        {
            return _key switch
            {
                "Description" => BOMDataViewModel.PartsFiltered.Select(p => p.Desc.ToString()).Distinct().ToList(),
                "Assem" => BOMDataViewModel.PartsFiltered.Select(p => p.Assem.ToString()).Distinct().ToList(),
                "Mark" => BOMDataViewModel.PartsFiltered.Select(p => p.Mark.ToString()).Distinct().ToList(),
                "Material" => BOMDataViewModel.PartsFiltered.Select(p => p.Material.ToString()).Distinct().ToList(),
                "Length" => BOMDataViewModel.PartsFiltered.Select(p => p.Length.ToString()).Distinct().ToList(),
                "Num" => BOMDataViewModel.PartsFiltered.Select(p => p.Num.ToString()).Distinct().ToList(),
                "WeightOne" => BOMDataViewModel.PartsFiltered.Select(p => p.WeightOne.ToString()).Distinct().ToList(),
                "PArea" => BOMDataViewModel.PartsFiltered.Select(p => p.PArea.ToString()).Distinct().ToList(),
                "WeightSum" => BOMDataViewModel.PartsFiltered.Select(p => p.WeightSum.ToString()).Distinct().ToList(),
                _ => new List<string>()
            };
        }

        //모두선택 체크박스 이벤트 <--------------------------------------------------------
        private static void SelectAllCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;
            // 체크된 경우, 모든 하위 체크박스를 체크 처리
            SetAllCheckBoxesChecked((checkBox.Parent!.Parent as Panel)!, true); 
        }

        private static void SelectAllCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;
            checkBox.IsThreeState = false;
            // 체크 해제된 경우, 모든 하위 체크박스를 체크 해제 처리
            SetAllCheckBoxesChecked((checkBox.Parent!.Parent as Panel)!, false);
        }

        private static void SetAllCheckBoxesChecked(Panel panel, bool isChecked)
        {
            foreach (var child in panel.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0]) // 첫 번째는 "모두 선택" 체크박스이므로 제외
                {
                    checkBox.IsThreeState = false;
                    checkBox.IsChecked = isChecked;
                }
        }//------------------------------------------------------------------------------>

        private static void FilterApply_Btn_Click(object? sender, RoutedEventArgs e)
        {
            // 적용 버튼 클릭 시 처리할 로직 작성
            if (sender is not Button applyButton) return;
            if (applyButton.Parent is not StackPanel stackPanel) return;
            var scrollViewer = stackPanel.Children.OfType<ScrollViewer>().FirstOrDefault();
            var panel = scrollViewer?.Content as StackPanel;

            SelectedItems.Clear();
            foreach (var child in panel!.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0] && checkBox.IsChecked == true)
                    SelectedItems.Add(checkBox.Content!.ToString()!);
            Console.WriteLine("선택된 아이템 개수: " + SelectedItems.Count);

            //모두 선택 체크박스가 체크되어있을 시 필터 헤제와 같으므로 적절한 처리 
            var selectAllCheckBox = panel.Children.OfType<Border>().FirstOrDefault()!.Child as CheckBox;
            BOMDataViewModel.ApplyFilter(applyButton.Tag?.ToString()!, selectAllCheckBox!.IsChecked == true);
            RefreshFilterSet();
        }

        //필터 적용버튼일 클릭될 때 현재 필터 뒤에 다른 필터가 적용중이면 모두 헤제 처리 
        private static void RefreshFilterSet()
        {
            var index = -1;
            // key의 인덱스 찾기
            if (filterSet.Count > 0)
            {
                for (var i = 0; i < filterSet.Count; i++)
                {
                    if (!filterSet.Cast<DictionaryEntry>().ElementAt(i).Key.Equals(_key)) continue;
                    index = i;
                    break;
                }
            }

            // 인덱스가 유효한 경우 이후의 모든 원소 제거
            if (index >= 0 && index < filterSet.Count - 1)
            {
                var i = filterSet.Count - 1;
                for (; i > index; i--)
                    filterSet.RemoveAt(i);
            }
        }
        private static void SelectCheckBoxes(Panel panel, string filter)
        {
            foreach (var child in panel.Children)
                if (child is CheckBox checkBox && checkBox != panel.Children[0])
                    checkBox.IsVisible = string.IsNullOrEmpty(filter) || checkBox.Content!.ToString()!.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
        
        private static bool GetAllSelectCheckState(FlyoutBase flyoutBase)
        {
            var filter = (Flyout)flyoutBase;
            var parentStackPanel = filter.Content as StackPanel;
            var scrollViewer = parentStackPanel?.Children.OfType<StackPanel>().FirstOrDefault()!.Children.OfType<ScrollViewer>().FirstOrDefault();
            var childStackPanel = scrollViewer!.Content as StackPanel;
            var selectAllCheckBox = childStackPanel!.Children.OfType<Border>().FirstOrDefault()!.Child as CheckBox;
            return selectAllCheckBox!.IsChecked == true;
        }
    }
}