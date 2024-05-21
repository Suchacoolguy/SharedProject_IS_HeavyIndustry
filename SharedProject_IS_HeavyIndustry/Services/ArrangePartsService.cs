using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    private static List<int> length_options_rawMaterial = new List<int>() {6010, 7010, 7510, 8010, 9510, 10010, 12110};
    private static ObservableCollection<RawMaterial> raw_materials_used = arrangeParts();
    
    public static ObservableCollection<RawMaterial> arrangeParts()
    {
        ObservableCollection<RawMaterial> raw_materials_used = new ObservableCollection<RawMaterial>();
        
        List<Part> part_list = ExcelDataLoader.PartListFromExcel("/Users/suchacoolguy/Documents/BOM_test.xlsx");
        // sort in descending order
        part_list.Sort((a, b) => b.Length.CompareTo(a.Length));
        // sort in descending order
        length_options_rawMaterial.Sort((a, b) => b.CompareTo(a));
        
        int best_length = length_options_rawMaterial[0];
        // iterate through the list of parts and create raw materials if needed
        for (int i = 0; i < part_list.Count; i++)
        {
            foreach (int rawLength in length_options_rawMaterial)
            {
                if (part_list[i].Length < rawLength && rawLength < best_length)
                {
                    best_length = rawLength;
                }
            }
            
            // if no raw material is created yet, create one
            if (raw_materials_used.Count == 0)
            {
                RawMaterial raw = new RawMaterial(best_length);
                raw.insert_part(part_list[i]);
                raw_materials_used.Add(raw);
                
                // removing an item while iterating through the list it belongs to is not recommended, so we don't remove the part here
                // part_list.Remove(part_list[i]);
            }
            // if there are raw materials already created
            else
            {
                bool part_added = false;
                // add the part to the first raw material that can fit it, if there is one
                foreach (var used_raw in raw_materials_used)
                {
                    if (used_raw.remaining_length >= part_list[i].Length)
                    {
                        used_raw.insert_part(part_list[i]);
                        part_added = true;
                        break;
                    }
                }
                
                // if the part was not added to any raw material, create a new raw material
                if (part_added == false)
                {
                    RawMaterial raw = new RawMaterial(best_length);
                    raw.insert_part(part_list[i]);
                    raw_materials_used.Add(raw);
                }
                
                // RawMaterial raw = new RawMaterial(best_length);
                // raw_materials_used.Add(raw);
            }
            
        }
        count_check(raw_materials_used);
        return raw_materials_used;
    }
    
    public static RawMaterial garra_creator(List<Part> part_list, List<int> part_length, int raw_length)
    {
        RawMaterial raw = new RawMaterial(raw_length);
        
        foreach (var required_len in part_length)
        {
            foreach (var part in part_list)
            {
                if (part.Length == required_len)
                {
                    raw.insert_part(part);
                    part_list.Remove(part);
                    break;
                }   
            }
        }    
        return raw;
    }

    public ObservableCollection<RawMaterial> GetArrangedRawMaterials()
    {
        return raw_materials_used;
    }
    
    public static List<int> GetLengthOptionsRawMaterial()
    {
        return length_options_rawMaterial;
    }

    public static void count_check(ObservableCollection<RawMaterial> rawMaterialUsed)
    {
        int count = 0;
        foreach (var raw in rawMaterialUsed)
        {
            foreach (var part in raw.PartsInside)
            {
                count++;
            }
        }
        
        Console.WriteLine("Total parts: " + count);
    }
}