using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace SharedProject_IS_HeavyIndustry.Models;

public class ExcelDataReader
{
    private static List<Part> parts = new List<Part>();
    public static ExcelPackage read(string filePath)
    {
        return new ExcelPackage(new FileInfo(filePath));
    }
        
    public static List<Part> PartListFromExcel(ExcelWorksheet worksheet)
    {
        var row = FindStartingRow(worksheet);
        
        while (row <= worksheet.Dimension.End.Row)
        {
            var cellValue = worksheet.Cells[row, 4].Value;
            if (cellValue != null && int.TryParse(cellValue.ToString(), out int intValue))
            {
                parts.Add(ExtractData(worksheet, row));
            }

            row++;
        }

        return parts;
    }

    private static int FindStartingRow(ExcelWorksheet worksheet)
    {
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

        return new Part(assem, mark, material, length, num, weightOne, weightSum, pArea, description);
    }
}