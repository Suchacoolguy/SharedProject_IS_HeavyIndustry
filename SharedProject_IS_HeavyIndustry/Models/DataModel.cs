using System;
using System.Collections.Generic;

namespace SharedProject_IS_HeavyIndustry.Models;

public class DataModel
{
    public static List<Part> parts;
    public static List<int> lengthOptionsRawMaterial;
    public int NumItems;
    public int NumBins;
    public int NumRawMaterialOptions;
    
    public DataModel(List<Part> partList, List<int> lengthOptions)
    {
        parts = partList;
        lengthOptionsRawMaterial = lengthOptions;
        NumItems = parts.Count;
        NumBins = parts.Count;
        NumRawMaterialOptions = lengthOptionsRawMaterial.Count;
        Console.WriteLine("Num: " + parts.Count);
    }
}