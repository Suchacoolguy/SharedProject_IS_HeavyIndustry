using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Views;
using OfficeOpenXml;
using System.IO;
using System.Text.RegularExpressions;
using SharedProject_IS_HeavyIndustry;
using SharedProject_IS_HeavyIndustry.Models;
using OfficeOpenXml.Style;

namespace SharedProject_IS_HeavyIndustry
{
    public class ExcelDataLoader
    {
        private static List<Part> parts = new List<Part>();
        private static List<Part> overSizeParts = new List<Part>();
        private int currentNum = 0;

        public static List<Part> PartListFromExcel(string filePath)
        {
            const string sheet = "IMB-현장"; // 원하는 시트 선택

            // Part temp = new Part();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[sheet];
                var row = FindStartingRow(worksheet);

                while (row <= worksheet.Dimension.End.Row)
                {
                    var cellValue = worksheet.Cells[row, 4].Value;
                    if (cellValue != null && int.TryParse(cellValue.ToString(), out int intValue))
                    {
                        Part temp = ExtractData(worksheet, row);
                        if (temp.Desc.Type.Equals("TB") && temp.Desc.Size.Equals("150*100*3.2"))
                        {
                            for (int i = temp.Num; i > 0; i--)
                            {
                                Part part = new Part(temp.Assem, temp.Mark, temp.Material, temp.Length, 1, temp.WeightOne, temp.WeightSum, temp.PArea, temp.Desc);
                                if (part.Length <= 10010)
                                    parts.Add(part);
                                else
                                    overSizeParts.Add(part);
                            }    
                        }
                    }
                    row++;
                }
            }
            overSizeParts.Sort((x, y) => x.Length.CompareTo(y.Length));
            return parts;
        }
        
        public static ObservableCollection<Part> GetOverSizeParts()
        {
            ObservableCollection<Part> res = new ObservableCollection<Part>(overSizeParts);
            return res;
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
            Description description = new Description(type, size);
            
            return new Part(assem, mark, material, length, num, weightOne, weightSum, pArea, description);
        }
    }
}