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
                        // Console.WriteLine(temp.Desc.Type.Equals("TB"));
                        // Console.WriteLine(temp.Desc.Size.Equals("150*100*3.2"));
                        // Console.WriteLine(temp.Desc.Size);
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
        
        public static void ExportDataToExcel(List<RawMaterial> rawMaterials, string filePath)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Raw Materials");

                // Set the header
                // worksheet.Cells[1, 1].Value = "NAME";
                // worksheet.Cells[1, 3].Value = "DESCRIPTION";
                // worksheet.Cells[1, 5].Value = "MATERIAL";
                // worksheet.Cells[1, 7].Value = "UNIT WEIGHT";
                // worksheet.Cells[2, 1].Value = "각관";
                // // worksheet.Cells[2, 3].Value = "";
                
                worksheet.Cells[1, 1].Value = "NO. ";
                worksheet.Cells[1, 2].Value = "STOCK";
                worksheet.Cells[1, 5].Value = "CUTTING PARTS";
                worksheet.Cells[1, 9].Value = "SCRAP";

                // Format the header
                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    // range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                }

                // Populate the data
                int currentRow = 2;
                for (int i = 0; i < rawMaterials.Count; i++)
                {
                    var rawMaterial = rawMaterials[i];
                    worksheet.Cells[currentRow, 1].Value = i + 1;
                    worksheet.Cells[currentRow, 2].Value = rawMaterial.Length;
                    worksheet.Cells[currentRow, 9].Value = rawMaterial.remaining_length;
                    var range = worksheet.Cells[currentRow, 3, currentRow, 8];
                    range.Merge = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    currentRow++;
                    for (int j = 0; j < rawMaterial.get_parts_inside().Count; j++)
                    {
                        worksheet.Cells[currentRow + j, 2].Value = j + 1;
                        worksheet.Cells[currentRow + j, 3].Value = "BlockMark:";
                        worksheet.Cells[currentRow + j, 5].Value = "DWGNo:";
                        worksheet.Cells[currentRow + j, 7].Value = "Length:";
                        worksheet.Cells[currentRow + j, 8].Value = rawMaterial.get_parts_inside()[j].Length;
                    }
                    currentRow += rawMaterial.get_parts_inside().Count;
                    currentRow++;
                }

                // AutoFit columns for all cells
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save the workbook
                package.SaveAs(new FileInfo(filePath));
            }
        }
    }
}