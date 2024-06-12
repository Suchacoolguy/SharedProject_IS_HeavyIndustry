using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;


namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class BOMDataViewModel
{
    public static string SheetName = null!;
    public static ObservableCollection<Part> AllParts { get; set; }
    public static ObservableCollection<Part> PartsForTask { get; set; } = [];
    public static ObservableCollection<Part> PartsToSeparate { get; set; } = [];
    
    public BOMDataViewModel(List<Part> parts)
    {
        AllParts = new ObservableCollection<Part>(parts);
    }
    
    public static List<Description> GetDescriptionList() // DNDTabViewModel에서 사용
    {
        var duplicationCheckSet = new HashSet<string>();
        var descList = new List<Description>();

        foreach (var part in AllParts) // 파트 목록을 순회하며 중복되지 않은 설명을 리스트에 추가
        {
            if (duplicationCheckSet.Add(part.Desc.ToString())) // HashSet에 현재 파트의 설명이 없는 경우 추가
                descList.Add(part.Desc);                           // HashSet에 성공적으로 추가된 경우만 리스트에 추가
        }

        return descList;
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