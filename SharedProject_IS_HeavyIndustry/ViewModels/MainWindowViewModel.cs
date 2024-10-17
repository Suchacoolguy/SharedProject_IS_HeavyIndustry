using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
#pragma warning restore CA1822 // Mark members as static
    
    public static IXLWorksheet Sheet = null!;
    public static XLWorkbook Workbook = null!;
    public static DragAndDropViewModel DragAndDropViewModel { get; set; }
    public static BOMDataViewModel BomDataViewModel { get; set; }
    //public static SettingsViewModel SettingsViewModel { get; set; }
    public static string ProjectName { get; set; } = "";
    public static string SelectedKey { get; set; } = "";
    public static Dictionary<string, ObservableCollection<RawMaterial?>> RawMaterialSet { get; set; } 
        = new Dictionary<string, ObservableCollection<RawMaterial?>>();
    
    public static Dictionary<string, ObservableCollection<Part>> TempPartSet { get; set; } 
        = new Dictionary<string, ObservableCollection<Part>>();

    public MainWindowViewModel()
    {
    }
    
    public void DropPartOntoRawMaterial(Part part, RawMaterial rawMaterial)
    {
        // Logic to add the part to the raw material
        rawMaterial.insert_part(part);
    }
    
    public static void ClearTempPartList()
    {
        DragAndDropViewModel.PartsCanNotBeArranged.Clear();
    }
    
    public static int CountTempPartList()
    {
        return DragAndDropViewModel.PartsCanNotBeArranged.Count;
    }
    
    // 이전 방식 key값을 전달받아 사용 but key값은 MainwindowViewModel에서 static으로 관리하기 때문에 인자로 전달 필요 없음
    public static void UpdateRawMaterialSet(ObservableCollection<RawMaterial?> rawMaterialSet)
    {
        if (RawMaterialSet.ContainsKey(SelectedKey))
            RawMaterialSet[SelectedKey] = rawMaterialSet;
        else
            RawMaterialSet.TryAdd(SelectedKey, rawMaterialSet);
    }
    
    public static void UpdateTempPartSet(ObservableCollection<Part> tempPartSet, string key)
    {
        if (TempPartSet.ContainsKey(key))
        {
            TempPartSet[key] = tempPartSet;
        }
        else
        {
            TempPartSet.TryAdd(key, tempPartSet);
        }
    }

    //레포트 출력 전에 RawMaterialSet 오름차순으로 정렬 
    public static void SortRawMaterialSet()
    {
        var sortedDic = RawMaterialSet.OrderBy(kv => ParseKey(kv.Key), new ParsedKeyComparer())
            .ToDictionary(kv => kv.Key, kv => kv.Value);
        RawMaterialSet = sortedDic;
    }

    private static (string materialKey, List<int> numbers) ParseKey(string key)
    {
        // 쉼표로 구분하여 앞부분을 prefix로, 뒷부분을 materialKey로 분리
        var parts = key.Split(',');

        string materialKey = parts[1];

        // materialKey에서 숫자를 추출하여 리스트로 변환
        List<int> numbers = materialKey.SkipWhile(c => !char.IsDigit(c))
            .Select(c => char.IsDigit(c) || c == '.' ? c : '*')
            .Aggregate("", (acc, ch) => acc + ch)
            .Split(new char[] { '*', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        return (materialKey, numbers);
    }

// 커스텀 Comparer 구현
    class ParsedKeyComparer : IComparer<(string materialKey, List<int> numbers)>
    {
        public int Compare((string materialKey, List<int> numbers) x, (string materialKey, List<int> numbers) y)
        {
            // 1. materialKey(쉼표 이후의 문자열) 비교
            int materialKeyComparison = string.Compare(x.materialKey, y.materialKey, StringComparison.Ordinal);
            if (materialKeyComparison != 0)
            {
                return materialKeyComparison;
            }

            // 2. numbers(숫자 부분) 비교
            for (int i = 0; i < Math.Min(x.numbers.Count, y.numbers.Count); i++)
            {
                int numberComparison = x.numbers[i].CompareTo(y.numbers[i]);
                if (numberComparison != 0)
                {
                    return numberComparison;
                }
            }

            // 3. 숫자 배열의 길이가 다를 경우 더 긴 쪽이 더 큼
            return x.numbers.Count.CompareTo(y.numbers.Count);
        }
    }

}