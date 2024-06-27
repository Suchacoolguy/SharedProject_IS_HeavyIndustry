    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using DynamicData;
    using SharedProject_IS_HeavyIndustry.Converters;
    using SharedProject_IS_HeavyIndustry.Models;

    namespace SharedProject_IS_HeavyIndustry.ViewModels
    {
        public class BOMDataViewModel 
        {
            public static string SheetName = null!;
            public static ObservableCollection<Part> AllParts { get; set; }
            public static ObservableCollection<Part> PartsForTask { get; set; } = []; // 제외 False, 분리 False
            public static ObservableCollection<Part> PartsToSeparate { get; set; } = []; // 제외 False, 분리 True
            
            //필터 적용 후 파트 
            public static ObservableCollection<Part> PartsFiltered { get; set; } = [];
            
            //필터 아이템
            public static List<string> OptionsForFiltering { get; set; } = [];

            private ObservableCollection<string> selectedFilterItems = [];
            
            public ObservableCollection<string> SelectedFilterItems
            {
                get => selectedFilterItems;
                set
                {
                    selectedFilterItems = value;
                    ApplyFilter();
                }
            }

            public BOMDataViewModel(List<Part> parts)
            {
                AllParts = new ObservableCollection<Part>(parts);
                PartsFiltered = Clone(AllParts);
                OptionsForFiltering = PartsFiltered.Select(p => p.Desc.ToString()).Distinct().ToList();
            }

            public static void ClassifyParts() // StartWindow, ExcelTabView에서 사용
            {
                foreach (var part in AllParts)
                {
                    if (part.IsExcluded) continue;
                    CopyPartList(part, part.NeedSeparate ? PartsToSeparate : PartsForTask);
                }
            }

            private static void CopyPartList(Part part, ObservableCollection<Part> list) // ClassifyParts에서 사용
            {
                if (part.Num > 1)
                {
                    for (var i = 0; i < part.Num; i++)
                    {
                        list.Add(new Part(part.Assem, part.Mark, part.Material, part.Length,
                            1, part.WeightOne, part.WeightSum, part.PArea, part.Desc));
                    }
                }
                else
                {
                    list.Add(part);
                }
            }

            private void ApplyFilter()
            {
                if (!SelectedFilterItems.Any())
                    PartsFiltered.Clear();
                else
                {
                    PartsFiltered.Clear();
                    foreach (var part in AllParts.Where(p => SelectedFilterItems.Contains(p.Desc.ToString())))
                        PartsFiltered.Add(part);
                }
            }

            private static ObservableCollection<Part> Clone(ObservableCollection<Part> list)
            {
                ObservableCollection<Part> result = [];
                foreach (var value in list)
                    result.Add(value);
                return result;
            }
        }
    }
