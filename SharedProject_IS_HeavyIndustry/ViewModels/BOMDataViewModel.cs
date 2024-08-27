    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics.X86;
    using DynamicData;
    using HarfBuzzSharp;
    using SharedProject_IS_HeavyIndustry.Converters;
    using SharedProject_IS_HeavyIndustry.Models;
    using SharedProject_IS_HeavyIndustry.Services;
    using SharedProject_IS_HeavyIndustry.Views.TabViews;

    namespace SharedProject_IS_HeavyIndustry.ViewModels
    {
        public class BOMDataViewModel 
        {
            public static string SheetName = null!;
            public static ObservableCollection<Part> AllParts { get; set; } = [];
            public static ObservableCollection<Part> PartsForTask { get; set; } = []; // 제외 False, 분리 False
            public static ObservableCollection<Part> PartsToSeparate { get; set; } = []; 

            
            //필터 적용 후 파트 
            public static ObservableCollection<Part> PartsFiltered { get; set; } = [];
            public static Stack<(string, ObservableCollection<Part>)> FilteredPartsStack = new ();
            public static bool ExcludeCheck = false, SeparateCheck = false, NeedSeparateCheck = false;
            
            public BOMDataViewModel(List<Part> parts)
            {
                Initialize();
                AllParts = new ObservableCollection<Part>(parts);
                PartsFiltered = Clone(AllParts);
            }

            private static void Initialize()
            {
                AllParts.Clear();
                PartsForTask.Clear();
                PartsToSeparate.Clear();
                PartsFiltered.Clear();
                FilteredPartsStack.Clear();
                ExcludeCheck = false;
                SeparateCheck = false;
                NeedSeparateCheck = false;
            }

            public static void ClassifyParts() // StartWindow, ExcelTabView에서 사용
            {
                if(PartsForTask.Any())
                    PartsForTask.Clear();
                if (PartsToSeparate.Any())
                    PartsToSeparate.Clear();
                foreach (var part in AllParts)
                {
                    if (part.IsExcluded) continue;
                    CopyPartList(part, part.IsOverLenth ? PartsToSeparate : PartsForTask);
                }
            }

            private static void CopyPartList(Part part, ObservableCollection<Part> list) // ClassifyParts에서 사용
            {
                if (part.Num > 1)
                    for (var i = 0; i < part.Num; i++)
                    {
                        var newPart = new Part(part.Assem, part.Mark, part.Material, part.Length,
                            1, part.WeightOne, part.WeightSum, part.PArea, part.Desc);
                        newPart.NeedSeparate = part.NeedSeparate;
                        newPart.lengthToBeSeparated = part.lengthToBeSeparated;
                        newPart.IsOverLenth = part.IsOverLenth;
                        list.Add(newPart);
                    }
                else
                    list.Add(part);
            }

            public static void ApplyFilter(string key, bool release)
            {
                var selectedFilterItems = FilteringService.SelectedItems;
                //Stack이 비어있으면 Allparts 가져와서 채움
                if (FilteredPartsStack.Count == 0)
                {
                    FilteredPartsStack.Push(("Base", Clone(AllParts)));
                    Console.WriteLine("스택 비어있음 원소 추가");
                }
                else
                {
                    //분리, 제외, 분리 필요 버튼이 활성화 되어있으면 삭제 
                    if (FilteredPartsStack.Peek().Item1.Equals("ToggleOption"))
                        FilteredPartsStack.Pop();
                    //Stack 최상위 원소가 현재 필터와 같은 경우 (ex-규격 필터를 설정하고 또 다시 규격 필터를 사용할 경우)
                    if (FilteredPartsStack.Peek().Item1.Equals(key))
                    {
                        FilteredPartsStack.Pop();
                        Console.WriteLine("최상위 원소 같은 POP실행");
                        Console.WriteLine("현재 스택 : " + FilteredPartsStack.Count);
                    }
                    //Stack에 현재 필터가 이미 존재하면 해당 필터가 나올때 까지 Pop
                    else if(FilteredPartsStack.Any(tuple => tuple.Item1.Equals(key)) || release)
                    {
                        while (!FilteredPartsStack.Pop().Item1.Equals(key))
                            continue;
                        Console.WriteLine("현재 필터가 이미 존재함 해당 필터 나올때 까지 POP실핼");
                        Console.WriteLine("현재 스택 : " + FilteredPartsStack.Count);
                    }
                    BOMDataTabView. OffSwitches();
                }
                
                PartsFiltered.Clear();
                switch (key)
                {
                    case "Description":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.Desc.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Assem":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.Assem)))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Mark":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.Mark)))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Material":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.Material)))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Length":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.Length.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "Num":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.Num.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "WeightOne":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.WeightOne.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }

                    case "PArea":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.PArea.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }
                    case "WeightSum":
                    {
                        foreach (var part in FilteredPartsStack.Peek().Item2
                                     .Where(p => selectedFilterItems.Contains(p.WeightSum.ToString())))
                            PartsFiltered.Add(part);
                        break;
                    }
                }
                //필터링 결과 스택에 푸쉬
                FilteredPartsStack.Push((key, Clone(PartsFiltered)));
            }

            public static void ApplyToggleFilter()
            {
                PartsFiltered.Clear();
                if (FilteredPartsStack.Count == 0)
                {
                    FilteredPartsStack.Push(("Base", Clone(AllParts)));
                    Console.WriteLine("스택 비어있음 원소 추가");
                }
                if (FilteredPartsStack.Peek().Item1.Equals("ToggleOption"))
                    FilteredPartsStack.Pop();
                foreach (var part in FilteredPartsStack.Peek().Item2
                             .Where(p => (!ExcludeCheck || p.IsExcluded) 
                                         && (!NeedSeparateCheck || p.IsOverLenth)
                                         && (!SeparateCheck || p.NeedSeparate)))
                    PartsFiltered.Add(part);
                
                FilteredPartsStack.Push(("ToggleOption", Clone(PartsFiltered)));
            }

            public static void ReleaseAllFilter()
            {
                PartsFiltered.Clear();
                FilteredPartsStack.Clear();
                BOMDataTabView.OffSwitches();
                FilteringService.Clear();
                foreach (var p in AllParts)
                    PartsFiltered.Add(p);
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
