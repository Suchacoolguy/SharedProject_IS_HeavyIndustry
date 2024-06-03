
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkiaSharp;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class ExcelDataWriter
    {
        private static readonly string FolderPath = @"C:Report";
        private static string filePath = $"{DateTime.Now:yyyy.MM.dd} 우전 GNF C1.xlsx";
        private static int _row = 13;
        private static int _imgWidth = 0;
        public static void Write(Dictionary<string, ObservableCollection<RawMaterial>> rawMaterialSet)
        {
            try
            {
                DirectoryCheck(); // 폴더 존재 여부 확인

                using (var package = new ExcelPackage())
                {
                    string type, size;
                    foreach (var kvp in rawMaterialSet)
                    {
                        _row = 13; // _row 변수 초기화
                        var worksheet = package.Workbook.Worksheets.Add(kvp.Key);
                        _imgWidth = ModifyCellWidth(worksheet);
                        
                        type = Regex.Match(kvp.Key, @"^[^\d]+").Value;
                        size = Regex.Match(kvp.Key, @"[\d\*\.]+").Value;
                        
                        MakeHeader(worksheet, type, size, kvp.Value);
                        MakeChart(worksheet, kvp.Value);
                    }

                    // Excel 파일 저장
                    package.SaveAs(new FileInfo(filePath));
                }

                OpenFile();
                Console.WriteLine("Excel 파일이 생성되고 열렸습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류: " + ex.Message);
            }
        }

        public static void OpenFile()
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류: " + ex.Message);
            }
        }

        private static void DirectoryCheck()
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류: " + ex.Message);
            }
        }

        private static void MakeHeader(ExcelWorksheet worksheet, string type, string size, ObservableCollection<RawMaterial> rawMaterials)
        {
            // foreach (var raw in rawMaterials)
            // {
            //     add codes to get the numbers for the header.
            // }
            
                // 엑셀 세로 폭 설정
                worksheet.Row(3).Height = worksheet.DefaultRowHeight / 2;
                worksheet.Row(4).Height = worksheet.DefaultRowHeight / 2;
                worksheet.Row(9).Height = worksheet.DefaultRowHeight / 2;
                worksheet.Row(10).Height = worksheet.DefaultRowHeight / 2;
                
                MakeUnderLine(worksheet, 3);
                MakeUnderLine(worksheet, 9);
                
                // 첫 행 데이터 추가
                worksheet.Cells[1, 1].Value = "NAME";
                worksheet.Cells[1, 3].Value = "DESCRIPTION";
                worksheet.Cells[1, 5].Value = "MATERIAL";
                worksheet.Cells[1, 7].Value = "UNIT WEIGHT";
                
                worksheet.Cells[2, 1].Value = type;
                worksheet.Cells[2, 3].Value = size;
                worksheet.Cells[2, 5].Value = "SS275";
                worksheet.Cells[2, 7].Value = "20.1";
                worksheet.Cells[2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[2, 8].Value = "kg/m";
                worksheet.Cells[2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[5, 1].Value = "NUMBER OF PART TYPE : ";
                worksheet.Cells[5, 4].Value = 1;
                worksheet.Cells[5, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[5, 6].Value = "TOTAL NUMBER OF PART : ";
                worksheet.Cells[5, 9].Value = 1;
                worksheet.Cells[5, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[6, 1].Value = "TOTAL STOCK WEIGHT = ";
                worksheet.Cells[6, 4].Value = 242 + " [ Kg]";
                worksheet.Cells[6, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[6, 6].Value = "TOTAL PART WEIGHT = ";
                worksheet.Cells[6, 9].Value = 201 + " [ Kg]";
                worksheet.Cells[6, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[7, 1].Value = "TOTAL RESIDUAL WEIGHT = ";
                worksheet.Cells[7, 6].Value = "TOTAL SCRAP WEIGHT = ";
                worksheet.Cells[7, 9].Value = 40 + " [ Kg]";
                worksheet.Cells[7, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                worksheet.Cells[8, 1].Value = "TOTAL BAR USAGE : ";
                worksheet.Cells[8, 4].Value = 83 + "%";
                worksheet.Cells[8, 6].Value = "PRINT DATE : ";
                worksheet.Cells[8, 8].Value = $"{DateTime.Now:yyyy.MM.dd hh:mm}";
                
                worksheet.Cells[11, 1].Value = "NO.";
                worksheet.Cells[11, 2].Value = "STOCK";
                worksheet.Cells[11, 5].Value = "CUTTING PARTS";
                worksheet.Cells[11, 9].Value = "SCRAP";
        }

        //헤더의 경계선 
        private static void MakeUnderLine(ExcelWorksheet worksheet, int row)
        {
            // 1열부터 9열까지 아래쪽 테두리 설정
            for (int col = 1; col <= 9; col++)
            {
                var range = worksheet.Cells[row, col];
                var border = range.Style.Border;
                border.Bottom.Style = ExcelBorderStyle.Thick;
            }
        }

        //원자재 테두리 
        private static void MakeBorder(ExcelWorksheet worksheet, int row)
        {
            
            // 병합할 범위 선택
            var mergedCells = worksheet.Cells[row, 3, row, 8];
            // 셀 병합
            mergedCells.Merge = true;
            
            // 테두리 설정할 셀 범위 선택 (3열부터 8열까지, 1행부터 10행까지)
            var range = worksheet.Cells[row, 3, row, 8];

            // 테두리 스타일 설정
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        private static void InsertImage(ExcelWorksheet worksheet, SKBitmap image, int row)
        {
            
            worksheet.Row(row).Height = (int)worksheet.Row(row).Height + 5; // 예시로 높이를 100으로 설정
            
            double mergedCellWidth = worksheet.Column(3).Width;
            double mergedCellHeight = worksheet.Row(row).Height;
            Console.WriteLine("height : " + mergedCellHeight);
            Console.WriteLine("width : " + mergedCellWidth);
            /*mergedCellWidth = (worksheet.Column(3).Width) * (256 / 7); // 6개의 셀을 병합한 너비*/
            mergedCellHeight = (worksheet.Row(row).Height * 96 / 72);
            
            using (var ms = new MemoryStream())
            {
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                var picture = worksheet.Drawings.AddPicture("image" + row, ms);
                picture.SetPosition(row - 1, 0, 2, 0);
                //picture.SetSize(_imgWidth, (int)mergedCellHeight);
                picture.SetSize(537, (int)mergedCellHeight + 2);
            }
        }
        
        

        private static void MakeChart(ExcelWorksheet worksheet, ObservableCollection<RawMaterial> rawMaterials)
        {
            var i = 1;
            foreach (var rawMaterial in rawMaterials)
            {
                worksheet.Cells[_row, 1].Value = i;
                worksheet.Cells[_row, 2].Value = rawMaterial.Length;
                worksheet.Cells[_row, 9].Value = rawMaterial.RemainingLength;
                worksheet.Cells[_row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[_row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[_row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                MakeBorder(worksheet, _row);
                InsertImage(worksheet, rawMaterial.GenerateBarChartImage(), _row);
                i++;
                _row++;
                
                int j = 1;
                foreach (var part in rawMaterial.PartsInside)
                {
                    worksheet.Cells[_row, 2].Value = j;
                    worksheet.Cells[_row, 3].Value = "BlockMark:";
                    worksheet.Cells[_row, 4].Value = part.Mark;
                    worksheet.Cells[_row, 5].Value = "DWGNo:";
                    worksheet.Cells[_row, 6].Value = part.Assem;
                    worksheet.Cells[_row, 7].Value = "Length:";
                    worksheet.Cells[_row, 8].Value = part.Length;
                    j++;
                    _row++;
                }

                _row++;
            }
        }

        private static int ModifyCellWidth(ExcelWorksheet sheet)
        {
            var cellWidth = sheet.Column(3).Width;
            cellWidth =(int)((cellWidth + 2) * 45);
            for (var col = 3; col <= 8; col++)
            {
                sheet.Column(col).Width = cellWidth / 45;
            }
            Console.WriteLine(cellWidth/45);
            return (int)cellWidth;
        }
    }
}
/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using SkiaSharp;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class ExcelDataWriter
    {
        private static string folderPath = @"/Users/suchacoolguy/Documents/";
        private static string filePath = $"{DateTime.Now:yyyy.MM.dd} 우전 GNF C1.xlsx";
        private static int row = 13;
        public static void Write(List<RawMaterial> bomList)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                DirectoryCheck();
                // Excel 시트 생성
                var worksheet = package.Workbook.Worksheets.Add("각관 150*100*3.2");

                MakeHeader(worksheet);
                
                MakeChart(worksheet, bomList);
                // Excel 파일 저장
                package.SaveAs(new FileInfo(filePath));
            }

            Console.WriteLine("Excel 파일이 생성되고 열렸습니다.");
        }

        public static void OpenFile()
        {
            Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
        }

        private static void DirectoryCheck()
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류: " + ex.Message);
            }
        }

        private static void MakeHeader(ExcelWorksheet worksheet)
        {
                // 엑셀 세로 폭 설정
                worksheet.Row(3).Height = worksheet.DefaultRowHeight / 2;
                worksheet.Row(4).Height = worksheet.DefaultRowHeight / 2;
                worksheet.Row(9).Height = worksheet.DefaultRowHeight / 2;
                worksheet.Row(10).Height = worksheet.DefaultRowHeight / 2;
                
                MakeUnderLine(worksheet, 3);
                MakeUnderLine(worksheet, 9);
                
                // 첫 행 데이터 추가
                worksheet.Cells[1, 1].Value = "NAME";
                worksheet.Cells[1, 3].Value = "DESCRIPTION";
                worksheet.Cells[1, 5].Value = "MATERIAL";
                worksheet.Cells[1, 7].Value = "UNIT WEIGHT";
                
                worksheet.Cells[2, 1].Value = "각관";
                worksheet.Cells[2, 3].Value = "150*100*3.2";
                worksheet.Cells[2, 5].Value = "SS275";
                worksheet.Cells[2, 7].Value = "20.1";
                worksheet.Cells[2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[2, 8].Value = "kg/m";
                worksheet.Cells[2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[5, 1].Value = "NUMBER OF PART TYPE : ";
                worksheet.Cells[5, 4].Value = 1;
                worksheet.Cells[5, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[5, 6].Value = "TOTAL NUMBER OF PART : ";
                worksheet.Cells[5, 9].Value = 14;
                worksheet.Cells[5, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[6, 1].Value = "TOTAL STOCK WEIGHT = ";
                worksheet.Cells[6, 4].Value = 242 + " [ Kg]";
                worksheet.Cells[6, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[6, 6].Value = "TOTAL PART WEIGHT = ";
                worksheet.Cells[6, 9].Value = 201 + " [ Kg]";
                worksheet.Cells[6, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[7, 1].Value = "TOTAL RESIDUAL WEIGHT = ";
                worksheet.Cells[7, 6].Value = "TOTAL SCRAP WEIGHT = ";
                worksheet.Cells[7, 9].Value = 40 + " [ Kg]";
                worksheet.Cells[7, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                
                worksheet.Cells[8, 1].Value = "TOTAL BAR USAGE : ";
                worksheet.Cells[8, 4].Value = 83 + "%";
                worksheet.Cells[8, 6].Value = "PRINT DATE : ";
                worksheet.Cells[8, 8].Value = $"{DateTime.Now:yyyy.MM.dd hh:mm}";
                
                worksheet.Cells[11, 1].Value = "NO.";
                worksheet.Cells[11, 2].Value = "STOCK";
                worksheet.Cells[11, 5].Value = "CUTTING PARTS";
                worksheet.Cells[11, 9].Value = "SCRAP";
        }

        //헤더의 경계선 
        private static void MakeUnderLine(ExcelWorksheet worksheet, int row)
        {
            // 1열부터 9열까지 아래쪽 테두리 설정
            for (int col = 1; col <= 9; col++)
            {
                var range = worksheet.Cells[row, col];
                var border = range.Style.Border;
                border.Bottom.Style = ExcelBorderStyle.Thick;
            }
        }

        //원자재 테두리 
        private static void MakeBorder(ExcelWorksheet worksheet, int row)
        {
            
            // 병합할 범위 선택
            var mergedCells = worksheet.Cells[row, 3, row, 8];
            // 셀 병합
            mergedCells.Merge = true;
            
            // 테두리 설정할 셀 범위 선택 (3열부터 8열까지, 1행부터 10행까지)
            var range = worksheet.Cells[row, 3, row, 8];

            // 테두리 스타일 설정
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        
        private static void InsertImage(ExcelWorksheet worksheet, SKBitmap image, int row)
        {
            using (var ms = new MemoryStream())
            {
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                var picture = worksheet.Drawings.AddPicture("image" + row, ms);
                picture.SetPosition(row - 1, 0, 4, 0);
                picture.SetSize(100, 27);
            }
        }


        private static void MakeChart(ExcelWorksheet worksheet, List<RawMaterial> list)
        {
            var i = 1;
            foreach (var bom in list)
            {
                worksheet.Cells[row, 1].Value = i;
                worksheet.Cells[row, 2].Value = bom.Length;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                MakeBorder(worksheet, row);
                InsertImage(worksheet, bom.GenerateBarChartImage(), row);
                i++;
                row++;
                
                int j = 1;
                foreach (var part in bom.PartsInside)
                {
                    worksheet.Cells[row, 2].Value = j;
                    worksheet.Cells[row, 3].Value = "BlockMark:";
                    worksheet.Cells[row, 4].Value = part.Mark;
                    worksheet.Cells[row, 5].Value = "DWGNo:";
                    worksheet.Cells[row, 6].Value = part.Assem;
                    worksheet.Cells[row, 7].Value = "Length:";
                    worksheet.Cells[row, 8].Value = part.Length;
                    j++;
                    row++;
                }

                row++;
            }
        }
    }
}*/