using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using OfficeOpenXml;

namespace SharedProject_IS_HeavyIndustry.Models;

public static class WorkManager
{
    private static ExcelPackage _package = null!;
    private static ExcelWorksheet _sheet = null!;
    public static string SheetName = null!, ExcelFilePath = null!;
    public static string? ProjectName = null!;
    public static ObservableCollection<Part> PartsOrigin { get; set; } = null!;
    public static ObservableCollection<Part> PartsForTask { get; set; } = null!;
    public static ObservableCollection<Part> PartsForSeparate { get; set; } = null!;

    public static Dictionary<string, ObservableCollection<RawMaterial>> DragAndDropSet { get; set; } = null!;

    public static List<string> GetSheetNames()
    {
        return _package.Workbook.Worksheets.Select(sh => sh.Name).ToList();
    }

    /*public static ObservableCollection<Part> GetPartsFromSheet(string sheetName)
    {
        return ExcelDataReader.PartListFromExcel(package.Workbook.Worksheets[sheetName]);
    }*/

    public static void SetSheet(string sheetName)
    {
        if (string.IsNullOrEmpty(sheetName)) return;
        _sheet = _package.Workbook.Worksheets[sheetName];
        //시트를 선택하면 시트의 part정보를 뽑아옴
        PartsOrigin = ExcelDataReader.PartListFromExcel(_sheet);
    }

    public static void ReadExcelPackage()
    {
        _package = ExcelDataReader.Read(ExcelFilePath);
    }
    
    public static List<Description> GetDescriptionList()
    {
        var duplicationCheckSet = new HashSet<string>();
        var descList = new List<Description>();

        foreach (var part in PartsOrigin) // 파트 목록을 순회하며 중복되지 않은 설명을 리스트에 추가
        {
            if (duplicationCheckSet.Add(part.Desc.ToString())) // HashSet에 현재 파트의 설명이 없는 경우 추가
                descList.Add(part.Desc);                           // HashSet에 성공적으로 추가된 경우만 리스트에 추가
        }

        return descList;
    }

    public static ObservableCollection<Part> FindPartsByDescription(Description desc, ObservableCollection<Part> parts)
    {
        var list = new ObservableCollection<Part>();
        
        foreach (var part in parts)
            if(part.Desc.Equals(desc))
                list.Add(part);
        
        return list;
    }

    public static void ClassifyParts()
    {
        PartsForTask = [];
        PartsForSeparate = [];
        
        foreach (var part in PartsOrigin)
        {
            if (part.IsExcluded) continue;
            CopyPartList(part, part.NeedSeparate ? PartsForSeparate : PartsForTask);
        }
    }

    private static void CopyPartList(Part part, ObservableCollection<Part> list)
    {
        if(part.Num > 1)
            for (var i = 0; i < part.Num; i++)
                list.Add(new Part(part.Assem, part.Mark, part.Material, part.Length,
                    1, part.WeightOne, part.WeightSum, part.PArea, part.Desc));
        else
            list.Add(part);
    }
}

