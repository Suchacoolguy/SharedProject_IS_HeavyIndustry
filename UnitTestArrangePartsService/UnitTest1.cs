using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace UnitTestArrangePartsService;

public class UnitTest1
{
    [Fact]
    public static void ArrangePartsService_DividePart_ReturnVoid()
    {
        // Open the workbook
        using (var workbook = new XLWorkbook(@"C:\Users\coolg\OneDrive\문서\아이에스중공업\240930 봉암리_가물량_BOM.xlsx"))
        {
            // Access the sheet by name
            var sheet = workbook.Worksheet("Dwg-list");

            // Retrieve parts from BOM
            List<Part> partsFromBOM = ExcelDataReader.PartListFromExcel(sheet);

            // Arrange
            int expected = 1204;
            // Act
            int actual = partsFromBOM.Count; 

            // Assert
            Assert.Equal(expected, actual);
        }
    }
    
    private int ReturnOne()
    {
        return 1;
    }
}