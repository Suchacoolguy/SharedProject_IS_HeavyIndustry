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
    public static double MaxLen = ArrangePartsService._lengthOptionsRawMaterial.Max();
    // public static double MaxLen = 8000;
    private static List<string> HyungGangList = new List<string>(){"H", "I", "L", "C", "ㄷ", "TB"};

    public static ExcelPackage Read(string filePath)
    {
        return new ExcelPackage(new FileInfo(filePath));
    }
        
    public static ObservableCollection<Part> PartListFromExcel(ExcelWorksheet worksheet)
    {
        var parts = new ObservableCollection<Part>();
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
        var assem = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
        var mark = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;
        var desc = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty;
        var length = Convert.ToInt32(worksheet.Cells[row, 4].Value);
        var num = Convert.ToInt32(worksheet.Cells[row, 5].Value);
        var weightOne = Convert.ToDouble(worksheet.Cells[row, 6].Value);
        var weightSum = Convert.ToDouble(worksheet.Cells[row, 7].Value);
        var pArea = Convert.ToDouble(worksheet.Cells[row, 8].Value);
        var material = worksheet.Cells[row, 9].Value?.ToString() ?? string.Empty;
        var type = Regex.Match(desc, @"^[^\d]+").Value;
        var size = Regex.Match(desc, @"[\d\*\.]+").Value;
        var description = new Description(type, size);
        //분리필요로 변경된 코드 
        var part = new Part(assem, mark, material, length, num, weightOne, weightSum, pArea, description);
        if (length > MaxLen)
            part.IsOverLenth = true;
        if (!HyungGangList.Contains(type))
            part.IsExcluded = true;

        return part;
    }
}