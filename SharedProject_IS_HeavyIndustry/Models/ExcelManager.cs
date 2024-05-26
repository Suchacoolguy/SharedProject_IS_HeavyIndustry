using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace SharedProject_IS_HeavyIndustry.Models;

using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

public class ExcelManager
{
    private static ExcelPackage package;
    private static ExcelWorksheets sheets;
    private ExcelWorksheet sheet;
    /*private static List<Part> parts = new List<Part>();*/

    public ExcelManager(string filePath)
    {
        package = ExcelDataReader.Read(filePath);
        sheets = package.Workbook.Worksheets;
    }

    public List<string> GetSheetNames()
    {
        return sheets.Select(sh => sh.Name).ToList();
    }

    public ObservableCollection<Part> GetPartsFromSheet(string sheetName)
    {
        return ExcelDataReader.PartListFromExcel(package.Workbook.Worksheets[sheetName]);
    }
}
