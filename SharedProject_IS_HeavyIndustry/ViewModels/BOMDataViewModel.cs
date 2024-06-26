using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharedProject_IS_HeavyIndustry.Converters;
using SharedProject_IS_HeavyIndustry.Models;


namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class BOMDataViewModel
{
    public static string SheetName = null!;
    public static ObservableCollection<Part> AllParts { get; set; }
    public static ObservableCollection<Part> PartsForTask { get; set; } = []; //제외 False, 분리 False
    public static ObservableCollection<Part> PartsToSeparate { get; set; } = []; // 제외 False, 분리 True
    public static ObservableCollection<Part> PartsFiltered { get; set; } = [];
    public static List<string> OptionsForFiltering { get; set; } = [];
    
    public BOMDataViewModel(List<Part> parts)
    {
        AllParts = new ObservableCollection<Part>(parts);
        
        
        // 그냥 = AllParts 하면 참조가 같아져서 문제가 생길라나 모르겠다는 (ㅇ..ㅇ;;)
        // PartsFiltered = new ObservableCollection<Part>(parts);
        PartsFiltered = AllParts;
        OptionsForFiltering = PartsFiltered.Select(p => p.Assem).Distinct().ToList();
    }
    
    public static ObservableCollection<Part> GetFilteredParts(string material, string desc, ObservableCollection<Part> parts)
    {
        var list = new ObservableCollection<Part>();

        foreach (var part in parts)
            if (part.Desc.ToString().Equals(desc) && part.Material.Equals(material))
                list.Add(part);

        return list;
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
        if(part.Num > 1)
            for (var i = 0; i < part.Num; i++)
                list.Add(new Part(part.Assem, part.Mark, part.Material, part.Length,
                    1, part.WeightOne, part.WeightSum, part.PArea, part.Desc));
        else
            list.Add(part);
    }
}