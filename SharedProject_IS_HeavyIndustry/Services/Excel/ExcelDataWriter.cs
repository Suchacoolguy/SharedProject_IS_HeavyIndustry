using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DynamicData;
using SharedProject_IS_HeavyIndustry.Converters;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabViewModels;
using SkiaSharp;

namespace SharedProject_IS_HeavyIndustry.Models
{
    public class ExcelDataWriter
    {
        private static string fileName = $"{DateTime.Now:yyyy.MM.dd} {MainWindowViewModel.ProjectName}.xlsx";
        private static int _row = 13;
        private static int _imgWidth = 0;
        
        public static void Write(Dictionary<string, ObservableCollection<RawMaterial?>> rawMaterialSet)
        {
            using (var workbook = new XLWorkbook())
            {
                string type, size;
                foreach (var kvp in rawMaterialSet)
                {
                    if (kvp.Value.Count == 0) continue;
                    // 파트배치 완료된 자슥들과 파트배치 불가능했던 자슥들을 합칠 리스트
                    ObservableCollection<RawMaterial> extendedCollection = new ObservableCollection<RawMaterial>();
                    
                    _row = 13; // _row 변수 초기화
                    var sheetName = ConvertSheetName(kvp.Key);
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    //_imgWidth = ModifyCellWidth(worksheet);
                    //worksheet.ColumnWidth = (int)worksheet.ColumnWidth + 1;
                    worksheet.RowHeight = (int)worksheet.RowHeight;

                    var match = Regex.Match(kvp.Key, @"^(.*),([A-Za-z]+)([\d\*\.]+)$");
                    var material = match.Groups[1].Value;
                    type = match.Groups[2].Value;
                    size = match.Groups[3].Value;
                    
                    // 일단 파트배치 완료된 애들부터 넣어주고~
                    extendedCollection!.AddRange(kvp.Value);
                    
                    // 파트배치 불가능했던 애들도 있으면 넣어주자~
                    if (MainWindowViewModel.TempPartSet.TryGetValue(kvp.Key, out var tempParts))
                    {
                        // 파트배치 불가능한 애들 찾아서 넣어줄 리스트 생성~
                        ObservableCollection<RawMaterial> toBeAdded = new ObservableCollection<RawMaterial>();
                        
                        // 최대 길이 찾아서 가져오기~
                        int maxLen = SettingsViewModel.GetMaxLen(type + size);
                        
                        foreach (var tempPart in tempParts)
                        {
                            var rawMaterial = new RawMaterial(maxLen);
                            rawMaterial.insert_part(tempPart);
                            toBeAdded.Add(rawMaterial);
                        }
                        extendedCollection.AddRange(toBeAdded);
                    }

                    MakeHeader(worksheet, type, type + size, material, kvp.Value);
                    MakeChart(worksheet, extendedCollection);
                }

                // 다운로드 폴더 경로 생성
                var downloadFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                var filePath = Path.Combine(downloadFolder, fileName);

                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        File.WriteAllBytes(filePath, memoryStream.ToArray());
                        OpenFile(filePath);
                    }
                }
                catch
                {
                    MessageService.Send("같은 이름의 엑셀 파일이 열려있습니다\n해당 작업을 종료하고 시도해주세요");
                }
            }

            Console.WriteLine("Excel 파일이 생성되고 열렸습니다.");
        }

        private static void OpenFile(string filePath)
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


        private static void MakeHeader(IXLWorksheet worksheet, string type, string desc, string material, ObservableCollection<RawMaterial?> rawMaterials)
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
            
            var hyungGangSet = SettingsViewModel.HyungGangSet;
            hyungGangSet.TryGetValue(type, out type!);

            var rowLengthSet = JsonConverter.ReadDictionaryFromJson();
            rowLengthSet!.TryGetValue(desc, out var value);
            var rawWeight = value!.Weight;

            worksheet.Cell(2, 1).Value = type;
            worksheet.Cell(2, 3).Value = desc;
            worksheet.Cell(2, 5).Value = material;
            worksheet.Cell(2, 7).Value = rawWeight;
            worksheet.Cell(2, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(2, 8).Value = "kg/m";
            worksheet.Cell(2, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var insideParts = 0;
            var totalRemaining = 0;
            var totalPartLen = 0;
            var totalStockLen = 0;
            foreach (var raw in rawMaterials)
            {
                insideParts += raw.PartsInside.Count;
                totalRemaining += raw.RemainingLength;
                totalPartLen += raw.Length - raw.RemainingLength;
                totalStockLen += raw.Length;
            }
            
            worksheet.Cell(5, 1).Value = "NUMBER OF PART TYPE : ";
            worksheet.Cell(5, 4).Value = 1;
            worksheet.Cell(5, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(5, 6).Value = "TOTAL NUMBER OF PART : ";
            worksheet.Cell(5, 9).Value = insideParts;
            worksheet.Cell(5, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(6, 1).Value = "TOTAL STOCK WEIGHT = ";
            worksheet.Cell(6, 4).Value = ((double)totalStockLen/1000*rawWeight).ToString("F2") + " [ Kg]";
            worksheet.Cell(6, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Cell(6, 6).Value = "TOTAL PART WEIGHT = ";
            worksheet.Cell(6, 9).Value = ((double)totalPartLen/1000*rawWeight).ToString("F2") + " [ Kg]";
            worksheet.Cell(6, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(7, 1).Value = "TOTAL RESIDUAL WEIGHT = ";
            worksheet.Cell(7, 6).Value = "TOTAL SCRAP WEIGHT = ";
            worksheet.Cell(7, 9).Value = ((double)totalRemaining/1000 * rawWeight).ToString("F2")  + " [ Kg]";
            worksheet.Cell(7, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var barUsage = 0.0;
            foreach (var r in rawMaterials)
                barUsage = barUsage + (double)(r.Length - r.RemainingLength) * 100 / r.Length;

            barUsage = barUsage / rawMaterials.Count;

            worksheet.Cell(8, 1).Value = "TOTAL BAR USAGE : ";
            worksheet.Cell(8, 4).Value = (int)barUsage + "%";
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

        private static void MakeChart(IXLWorksheet worksheet, ObservableCollection<RawMaterial> rawMaterials)
        {
            var i = 1;
            foreach (var rawMaterial in rawMaterials)
            {
                worksheet.Cell(_row, 1).Value = i;
                worksheet.Cell(_row, 2).Value = rawMaterial.Length;
                worksheet.Cell(_row, 9).Value = rawMaterial.RemainingLength;
                var lengthCell = worksheet.Cell(_row, 9);
                lengthCell.Value = rawMaterial.RemainingLength;

                // part.Length가 음수일 경우 텍스트 색깔을 빨간색으로 설정
                if (rawMaterial.RemainingLength < 0)
                    lengthCell.Style.Font.FontColor = XLColor.Red;

                worksheet.Cell(_row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(_row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(_row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(_row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(_row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(_row, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                //MakeBorder(worksheet, _row);
                InsertImage(worksheet, rawMaterial.GenerateBarChartImage(), _row);
                i++;
                _row++;

                var j = 1;
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
            // 병합할 범위 선택
            var mergedCells = worksheet.Range(row, 3, row, 8);
            // 셀 병합
            mergedCells.Merge();
            // Calculate the dimensions of the merged cell
            double cellWidth = 0;
            for (int col = 3; col <= 8; col++)
            {
                cellWidth += worksheet.Column(col).Width;
            }

            double cellHeight = worksheet.Row(row).Height;

            // Convert cell dimensions from points to pixels (assuming 96 DPI)
            double cellWidthPx = cellWidth * 7.5; // Approximate conversion factor
            double cellHeightPx = cellHeight * 1.33; // Approximate conversion factor

            // Insert the image and scale it to fit the cell
            using (var ms = new MemoryStream())
            {
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                var picture = worksheet.AddPicture(ms)
                    .MoveTo(worksheet.Cell(row, 3))
                    .ScaleWidth(cellWidthPx / image.Width)
                    .ScaleHeight(cellHeightPx / image.Height);

                worksheet.Row(_row).Height += 10;

                // Adjust the position to fit within the cell
                picture.MoveTo(worksheet.Cell(row, 3), worksheet.Cell(row, 8).CellBelow().CellRight());
                picture.Placement = XLPicturePlacement.MoveAndSize;
            }
        }

        private static string ConvertSheetName(string input)
        {
            // 정규 표현식으로 입력 문자열을 그룹화하여 추출
            var match = Regex.Match(input, @"^(.*),([A-Za-z]+)([\d\*\.]+)$");
            if (!match.Success) return input;
            var hyungGangSet = SettingsViewModel.HyungGangSet;
    
            var material = match.Groups[1].Value;
            var type = match.Groups[2].Value;
            hyungGangSet.TryGetValue(type, out type);
            var dimensions = match.Groups[3].Value.Replace("*", "x");
    
            // 최종 문자열 생성
            var result = $"{type}({dimensions})_<{material}>";
    
            // 문자열이 31자 이상이면 뒤에서부터 자르기
            if (result.Length > 31)
            {
                result = result.Substring(0, 31);
            }

            return result;
        }

    }
}