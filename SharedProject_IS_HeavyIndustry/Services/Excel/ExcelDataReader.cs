using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OfficeOpenXml;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.Models;

public static class ExcelDataReader
{
    
    // public static double MaxLen = 8000;
    

    public static ExcelPackage Read(string filePath)
    {
        return new ExcelPackage(new FileInfo(filePath));
    }
        
    public static List<Part> PartListFromExcel(ExcelWorksheet worksheet)
    {
        var parts = new List<Part>();
        var row = FindStartingRow(worksheet);
        
        while (row <= worksheet.Dimension.End.Row)
        {
            var cellValue = worksheet.Cells[row, 4].Value;
            
            if (cellValue != null && int.TryParse(cellValue.ToString(), out int intValue))
                parts.Add(ExtractData(worksheet, row));

            row++;
        }

        return parts;
    }

    public static Dictionary<string, RawLengthSet> RawLengthSettingsFromExcel(string filePath)
    {
        var dictionery = new Dictionary<string, RawLengthSet>();

        var fileInfo = new FileInfo(filePath);
        using var package = new ExcelPackage(fileInfo);
        var worksheet = package.Workbook.Worksheets[0];

        var rowCount = worksheet.Dimension.Rows;
        for (var row = 1; row <= rowCount; row++) 
        {
            try
            {
                var description = worksheet.Cells[row, 2].Text;
                var weight = double.Parse(worksheet.Cells[row, 3].Text);
                var lengths = worksheet.Cells[row, 4].Text;

                var rawMaterialLengthSetting = new RawLengthSet(description, weight, lengths);
                dictionery.TryAdd(description, rawMaterialLengthSetting);
            }
            catch
            {
                //Console.WriteLine($"에러 확인 코드 - ExcelDataReader.cs RawLengthSettingsFromExcel() 읽을 수 없는 행: {row}");
                // 예외가 발생하더라도 무시하고 다음 행을 계속 처리
            }
        }
        return dictionery;
    }

    
    

    private static int FindStartingRow(ExcelWorksheet worksheet)
    {
        if (worksheet == null)
        {
            throw new ArgumentNullException(nameof(worksheet));
        }
        
        var row = 1;
        for (; row < worksheet.Dimension.End.Row; row++)
        {
            if (worksheet.Cells[row, 1].Value == null ||
                worksheet.Cells[row, 1].Value.ToString() != "ASSEM") continue;
            row++;
            break;
        }
        return row;
    }
    private static Part ExtractData(ExcelWorksheet worksheet, int row)
    { 
        var assem = (worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty).Trim();
        var mark = (worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty).Trim();
        var desc = (worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty).Trim();
        var length = Convert.ToInt32(worksheet.Cells[row, 4].Value);
        var num = Convert.ToInt32(worksheet.Cells[row, 5].Value);
        var weightOne = Convert.ToDouble(worksheet.Cells[row, 6].Value);
        var weightSum = Convert.ToDouble(worksheet.Cells[row, 7].Value);
        var pArea = Convert.ToDouble(worksheet.Cells[row, 8].Value);
        var material = (worksheet.Cells[row, 9].Value?.ToString() ?? string.Empty).Trim();
        var type = Regex.Match(desc, @"^[^\d]+").Value;
        var size = Regex.Match(desc, @"[\d\*\.]+").Value;
        var description = new Description(type, size);
        //분리필요로 변경된 코드 
        var part = new Part(assem, mark, material, length, num, weightOne, weightSum, pArea, description);
        
        Console.WriteLine(type);
        
        foreach (var hyungGangType in SettingsViewModel.HyungGangList)
        {
            // 형강 목록 리스트에 없으면 제외 True, 있으면 제외 False.
            if (type.Equals(hyungGangType.Trim()))
                part.IsExcluded = false;
        }
        if (!part.IsExcluded && length > SettingsViewModel.GetMaxLen(description.ToString()))
            part.IsOverLenth = true;

        return part;
    }
}