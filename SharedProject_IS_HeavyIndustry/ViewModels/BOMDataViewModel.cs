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
            
            public BOMDataViewModel(List<Part> parts)
            {
                AllParts = new ObservableCollection<Part>(parts);
                PartsFiltered = Clone(AllParts);
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
                    for (var i = 0; i < part.Num; i++)
                        list.Add(new Part(part.Assem, part.Mark, part.Material, part.Length,
                            1, part.WeightOne, part.WeightSum, part.PArea, part.Desc));
                else
                    list.Add(part);
            }

            public static void ApplyFilter(string type, List<string> selectedFilterItems)
            {

                PartsFiltered.Clear();
                switch (type)
                {
                    case "Description":
                    {
                        foreach (var part in AllParts.Where(p => selectedFilterItems.Contains(p.Desc.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Assem":
                    {
                        foreach (var part in AllParts.Where(p => selectedFilterItems.Contains(p.Assem)))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Mark":
                    {
                        foreach (var part in AllParts.Where(p => selectedFilterItems.Contains(p.Mark)))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Material":
                        foreach (var part in AllParts.Where(p => selectedFilterItems.Contains(p.Material)))
                            PartsFiltered.Add(part);
                        break;
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
