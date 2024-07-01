using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public static class ExcelDataReader
    {
        public static XLWorkbook Read(string filePath)
        {
            try
            {
                return new XLWorkbook(filePath);
            }
            catch (Exception e)
            {
                MessageService.Send("해당 엑셀파일이 사용중입니다.");
                return null!;
            }
        }
        
        public static List<Part> PartListFromExcel(IXLWorksheet worksheet)
        {
            var parts = new List<Part>();
            var row = FindStartingRow(worksheet);
            
            while (row <= worksheet.LastRowUsed()!.RowNumber())
            {
                var cellValue = worksheet.Cell(row, 4).Value.ToString();
                
                if (cellValue != null && int.TryParse(cellValue, out var intValue))
                    parts.Add(ExtractData(worksheet, row));

                row++;
            }

            return parts;
        }

        public static Dictionary<string, RawLengthSet> RawLengthSettingsFromExcel(string filePath)
        {
            Console.WriteLine(filePath);
            var dictionary = new Dictionary<string, RawLengthSet>();

            using var workbook = Read(filePath);
            if (workbook == null)
                return new();
            var worksheet = workbook.Worksheets.First();

            var rowCount = worksheet.LastRowUsed().RowNumber();
            for (var row = 1; row <= rowCount; row++) 
            {
                try
                {
                    var description = worksheet.Cell(row, 2).GetString();
                    var weight = double.Parse(worksheet.Cell(row, 3).GetString());
                    var lengths = worksheet.Cell(row, 4).GetString();

                    var rawMaterialLengthSetting = new RawLengthSet(description, weight, lengths);
                    dictionary.TryAdd(description, rawMaterialLengthSetting);
                }
                catch
                {
                    // 예외가 발생하더라도 무시하고 다음 행을 계속 처리
                }
            }
            return dictionary;
        }

        private static int FindStartingRow(IXLWorksheet worksheet)
        {
            if (worksheet == null)
            {
                throw new ArgumentNullException(nameof(worksheet));
            }
            
            var row = 1;
            for (; row < worksheet.LastRowUsed()!.RowNumber(); row++)
            {
                if (worksheet.Cell(row, 1).GetString() != "ASSEM") continue;
                row++;
                break;
            }
            return row;
        }

        private static Part ExtractData(IXLWorksheet worksheet, int row)
        { 
            var assem = (worksheet.Cell(row, 1).GetString() ?? string.Empty).Trim();
            var mark = (worksheet.Cell(row, 2).GetString() ?? string.Empty).Trim();
            var desc = (worksheet.Cell(row, 3).GetString() ?? string.Empty).Trim();
            int.TryParse(worksheet.Cell(row, 4).Value.ToString(), out var length);
            int.TryParse(worksheet.Cell(row, 5).Value.ToString(), out var num);
            double.TryParse(worksheet.Cell(row, 6).Value.ToString(), out var weightOne);
            double.TryParse(worksheet.Cell(row, 7).Value.ToString(), out var weightSum);
            double.TryParse(worksheet.Cell(row, 8).Value.ToString(), out var pArea);
            var material = (worksheet.Cell(row, 9).GetString() ?? string.Empty).Trim();
            var type = Regex.Match(desc, @"^[^\d]+").Value;
            var size = Regex.Match(desc, @"[\d\*\.]+").Value;
            var description = new Description(type, size);
            //분리필요로 변경된 코드 
            var part = new Part(assem, mark, material, length, num, weightOne, weightSum, pArea, description);
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
}
/*using System;
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
}*/