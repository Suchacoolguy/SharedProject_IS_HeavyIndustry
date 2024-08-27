using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public static class ExcelDataReader
    {
        private static string noRawLengthData = "";
        private static bool missingDataCheck = false;
        public static XLWorkbook Read(string filePath)
        {
            try
            {
                return new XLWorkbook(filePath);
            }
            catch (Exception e)
            {
                MessageService.Send("해당 엑셀파일이 사용중입니다.");
                Console.WriteLine(e);
                return null!;
            }
        }
        
        public static List<Part> PartListFromExcel(IXLWorksheet worksheet)
        {
            missingDataCheck = false;
            var parts = new List<Part>();
            var row = FindStartingRow(worksheet);
            
            while (row <= worksheet.LastRowUsed()!.RowNumber())
            {
                var cellValue = worksheet.Cell(row, 4).Value.ToString();
                
                if (cellValue != null && int.TryParse(cellValue, out var intValue))
                    parts.Add(ExtractData(worksheet, row));

                row++;
            }
            if(missingDataCheck)
                MessageService.Send("누락된 규격 정보가 있습니다\n");

            return parts;
        }

        public static Dictionary<string, RawLengthSet> RawLengthSettingsFromExcel(string filePath)
        {
            var dictionary = new Dictionary<string, RawLengthSet>();

            using var workbook = Read(filePath);
            if (workbook == null)
                return new();
            var worksheet = workbook.Worksheets.First();

            var rowCount = worksheet!.LastRowUsed()!.RowNumber();
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
            foreach (var hyungGangType in SettingsViewModel.GetHyungGangList())
            {
                // 형강 목록 리스트에 없으면 제외 True, 있으면 제외 False.
                if (type.Equals(hyungGangType.Trim()))
                    part.IsExcluded = false;
            }
            //형강목록에 없는 형강 알림윈도우에 띄움
            if (part.IsExcluded)
                AlarmWindowViewModel.AddToMissingHyungGangBuffer(type);
            
            //규격목록에 길이가 설정 되어있지 않다면 제외
            if (!part.IsExcluded && SettingsViewModel.GetLengthOption(desc.ToString()).Count < 1)
            {
                part.IsExcluded = true;
                //noRawLengthData = part.Desc + "\n";
                RawStandardViewModel.AddToMissingStandardBuffer(part.Desc.ToString().Replace(" ", ""));
                missingDataCheck = true;
            }
            else if (!part.IsExcluded && length > SettingsViewModel.GetMaxLen(description.ToString()))
                part.IsOverLenth = true;

            
            return part;
        }
    }
}