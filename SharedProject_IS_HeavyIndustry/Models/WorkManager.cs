using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OfficeOpenXml;

namespace SharedProject_IS_HeavyIndustry.Models;

public static class WorkManager
{
    private static ExcelPackage package;
    private static ExcelWorksheet sheet;
    public static string SheetName = null!, ExcelFilePath = null!;
    public static ObservableCollection<Part> PartsOrigin { get; set; }
    
    public static List<string> GetSheetNames()
    {
        return package.Workbook.Worksheets.Select(sh => sh.Name).ToList();
    }

    /*public static ObservableCollection<Part> GetPartsFromSheet(string sheetName)
    {
        return ExcelDataReader.PartListFromExcel(package.Workbook.Worksheets[sheetName]);
    }*/

    public static void SetSheet(string sheetName)
    {
        if (string.IsNullOrEmpty(sheetName)) return;
        sheet = package.Workbook.Worksheets[sheetName];
        //시트를 선택하면 시트의 part정보를 뽑아옴
        PartsOrigin = ExcelDataReader.PartListFromExcel(sheet);
    }

    public static void ReadExcelPackage()
    {
        package = ExcelDataReader.Read(ExcelFilePath);
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

    public static List<Part> FindPartsByDescription(Description desc)
    {
        var list = new List<Part>();
        
        foreach (var part in PartsOrigin)
            if(part.Desc.Equals(desc))
                list.Add(part);
        
        return list;
    }
}

