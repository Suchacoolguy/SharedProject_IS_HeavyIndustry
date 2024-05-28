using System.Collections.Generic;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Services;

public class BOMDataService
{
    List<Part> part_list = ExcelDataLoader.PartListFromExcel(@"C:\ISProject\forDemo.xlsx");
    
    public List<Part> GetPartList()
    {
        return part_list;
    }
}