using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using SkiaSharp;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class ExcelDataWriter
    {
        private static string fileName = $"{DateTime.Now:yyyy.MM.dd} 우전 GNF C1.xlsx";
        private static int _row = 13;
        private static int _imgWidth = 0;

        public static void Write(Dictionary<string, ObservableCollection<RawMaterial>> rawMaterialSet)
        {
            using (var workbook = new XLWorkbook())
            {
                string type, size;
                foreach (var kvp in rawMaterialSet)
                {
                    _row = 13; // _row 변수 초기화
                    var sheetName = ConvertSheetName(kvp.Key);
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    _imgWidth = ModifyCellWidth(worksheet);

                    type = Regex.Match(kvp.Key, @"^[^\d]+").Value;
                    size = Regex.Match(kvp.Key, @"[\d\*\.]+").Value;

                    MakeHeader(worksheet, type, size, kvp.Value);
                    MakeChart(worksheet, kvp.Value);
                }

                // 메모리 스트림에 엑셀 파일 저장
                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // 임시 파일 경로 생성
                    File.WriteAllBytes(fileName, memoryStream.ToArray());

                    // 엑셀 파일 열기
                    OpenFile(fileName);
                }
            }

            Console.WriteLine("Excel 파일이 생성되고 열렸습니다.");
        }

        public static void OpenFile(string filePath)
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


        private static void MakeHeader(IXLWorksheet worksheet, string type, string size, ObservableCollection<RawMaterial> rawMaterials)
        {
            // 엑셀 세로 폭 설정
            worksheet.Row(3).Height = worksheet.RowHeight / 2;
            worksheet.Row(4).Height = worksheet.RowHeight / 2;
            worksheet.Row(9).Height = worksheet.RowHeight / 2;
            worksheet.Row(10).Height = worksheet.RowHeight / 2;

            MakeUnderLine(worksheet, 3);
            MakeUnderLine(worksheet, 9);

            // 첫 행 데이터 추가
            worksheet.Cell(1, 1).Value = "NAME";
            worksheet.Cell(1, 3).Value = "DESCRIPTION";
            worksheet.Cell(1, 5).Value = "MATERIAL";
            worksheet.Cell(1, 7).Value = "UNIT WEIGHT";

            worksheet.Cell(2, 1).Value = type;
            worksheet.Cell(2, 3).Value = size;
            worksheet.Cell(2, 5).Value = "SS275";
            worksheet.Cell(2, 7).Value = "20.1";
            worksheet.Cell(2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(2, 8).Value = "kg/m";
            worksheet.Cell(2, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(5, 1).Value = "NUMBER OF PART TYPE : ";
            worksheet.Cell(5, 4).Value = 1;
            worksheet.Cell(5, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(5, 6).Value = "TOTAL NUMBER OF PART : ";
            worksheet.Cell(5, 9).Value = 1;
            worksheet.Cell(5, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(6, 1).Value = "TOTAL STOCK WEIGHT = ";
            worksheet.Cell(6, 4).Value = 242 + " [ Kg]";
            worksheet.Cell(6, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(6, 6).Value = "TOTAL PART WEIGHT = ";
            worksheet.Cell(6, 9).Value = 201 + " [ Kg]";
            worksheet.Cell(6, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(7, 1).Value = "TOTAL RESIDUAL WEIGHT = ";
            worksheet.Cell(7, 6).Value = "TOTAL SCRAP WEIGHT = ";
            worksheet.Cell(7, 9).Value = 40 + " [ Kg]";
            worksheet.Cell(7, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(8, 1).Value = "TOTAL BAR USAGE : ";
            worksheet.Cell(8, 4).Value = 83 + "%";
            worksheet.Cell(8, 6).Value = "PRINT DATE : ";
            worksheet.Cell(8, 8).Value = $"{DateTime.Now:yyyy.MM.dd hh:mm}";

            worksheet.Cell(11, 1).Value = "NO.";
            worksheet.Cell(11, 2).Value = "STOCK";
            worksheet.Cell(11, 5).Value = "CUTTING PARTS";
            worksheet.Cell(11, 9).Value = "SCRAP";
        }

        //헤더의 경계선 
        private static void MakeUnderLine(IXLWorksheet worksheet, int row)
        {
            // 1열부터 9열까지 아래쪽 테두리 설정
            for (int col = 1; col <= 9; col++)
            {
                var range = worksheet.Cell(row, col);
                var border = range.Style.Border;
                border.BottomBorder = XLBorderStyleValues.Thick;
            }
        }

        //원자재 테두리 
        private static void MakeBorder(IXLWorksheet worksheet, int row)
        {
            // 병합할 범위 선택
            var mergedCells = worksheet.Range(row, 3, row, 8);
            // 셀 병합
            mergedCells.Merge();

            // 테두리 설정할 셀 범위 선택 (3열부터 8열까지, 1행부터 10행까지)
            var range = worksheet.Range(row, 3, row, 8);

            // 테두리 스타일 설정
            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        private static void InsertImage(IXLWorksheet worksheet, SKBitmap image, int row)
        {
            worksheet.Row(row).Height = worksheet.Row(row).Height + 5; // 예시로 높이를 100으로 설정

            double mergedCellWidth = worksheet.Column(3).Width;
            double mergedCellHeight = worksheet.Row(row).Height;
            Console.WriteLine("height : " + mergedCellHeight);
            Console.WriteLine("width : " + mergedCellWidth);
            mergedCellHeight = (worksheet.Row(row).Height * 96 / 72);

            using (var ms = new MemoryStream())
            {
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                var picture = worksheet.AddPicture(ms).MoveTo(worksheet.Cell(row, 3));
                picture.ScaleWidth(1.0);
                picture.ScaleHeight((mergedCellHeight + 2) / picture.Height);
            }
        }

        private static void MakeChart(IXLWorksheet worksheet, ObservableCollection<RawMaterial> rawMaterials)
        {
            var i = 1;
            foreach (var rawMaterial in rawMaterials)
            {
                worksheet.Cell(_row, 1).Value = i;
                worksheet.Cell(_row, 2).Value = rawMaterial.Length;
                worksheet.Cell(_row, 9).Value = rawMaterial.RemainingLength;
                worksheet.Cell(_row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(_row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(_row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                MakeBorder(worksheet, _row);
                InsertImage(worksheet, rawMaterial.GenerateBarChartImage(), _row);
                i++;
                _row++;

                int j = 1;
                foreach (var part in rawMaterial.PartsInside)
                {
                    worksheet.Cell(_row, 2).Value = j;
                    worksheet.Cell(_row, 3).Value = "BlockMark:";
                    worksheet.Cell(_row, 4).Value = part.Mark;
                    worksheet.Cell(_row, 5).Value = "DWGNo:";
                    worksheet.Cell(_row, 6).Value = part.Assem;
                    worksheet.Cell(_row, 7).Value = "Length:";
                    worksheet.Cell(_row, 8).Value = part.Length;
                    j++;
                    _row++;
                }

                _row++;
            }
        }

        private static int ModifyCellWidth(IXLWorksheet sheet)
        {
            var cellWidth = sheet.Column(3).Width;
            cellWidth = (int)((cellWidth + 2) * 45);
            for (var col = 3; col <= 8; col++)
            {
                sheet.Column(col).Width = cellWidth / 45;
            }
            Console.WriteLine(cellWidth / 45);
            return (int)cellWidth;
        }
        public static string ConvertSheetName(string input)
        {
            // 정규 표현식으로 입력 문자열을 그룹화하여 추출
            var match = Regex.Match(input, @"^(.*),([A-Za-z]+)([\d\*\.]+)$");

            if (match.Success)
            {
                string material = match.Groups[1].Value;
                string type = match.Groups[2].Value;
                string dimensions = match.Groups[3].Value.Replace("*", "x");

                // 변환된 형식의 문자열 생성
                return $"{type}({dimensions})_<{material}>";
            }

            // 입력 형식이 맞지 않는 경우 원래 문자열 반환
            return input;
        }

    }
}
/*
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
        private static string _filePath = $"{DateTime.Now:yyyy.MM.dd} 우전 GNF C1.xlsx";
        private static int _row = 13;
        private static int _imgWidth = 0;
        public static void Write(Dictionary<string, ObservableCollection<RawMaterial>> rawMaterialSet)
        {
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
                package.SaveAs(new FileInfo(_filePath));
            }

            OpenFile();
            Console.WriteLine("Excel 파일이 생성되고 열렸습니다.");
        }

        public static void OpenFile()
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = _filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류: " + ex.Message);
            }
        }

        private static void MakeHeader(ExcelWorksheet worksheet, string type, string size, ObservableCollection<RawMaterial> rawMaterials)
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
            /*mergedCellWidth = (worksheet.Column(3).Width) * (256 / 7); // 6개의 셀을 병합한 너비#1#
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
}*/