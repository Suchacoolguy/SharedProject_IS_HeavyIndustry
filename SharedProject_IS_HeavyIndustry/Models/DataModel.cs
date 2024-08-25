using System;
using System.Collections.Generic;

namespace SharedProject_IS_HeavyIndustry.Models;

public class DataModel
{
    public static List<Part>? parts;
    public static List<int>? lengthOptionsRawMaterial;
    public int NumItems;
    public int NumBins;
    public int NumRawMaterialOptions;
    
    public DataModel(List<Part>? partList, List<int>? lengthOptions)
    {
        parts = partList;
        lengthOptionsRawMaterial = lengthOptions;
        if (parts != null)
            NumItems = parts.Count;
        if (parts != null)
            NumBins = parts.Count;
        if (lengthOptionsRawMaterial != null)
            NumRawMaterialOptions = lengthOptionsRawMaterial.Count;
    }

    public DataModel(List<Part>? partList, List<int>? lengthOptions, int numBins)
    {
        parts = partList;
        lengthOptionsRawMaterial = lengthOptions;
        if (parts != null)
            NumItems = parts.Count;
        NumBins = numBins;
        if (lengthOptionsRawMaterial != null)
            NumRawMaterialOptions = lengthOptionsRawMaterial.Count;
        if (parts != null)
            Console.WriteLine("Num: " + parts.Count);
    }
}