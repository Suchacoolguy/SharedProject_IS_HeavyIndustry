using System;
using System.Collections.Generic;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Services;

public class ArrangePartsService
{
    private static List<int> length_options_rawMaterial = new List<int>() {6010, 7010, 7510, 8010, 9510, 10010, 12110};
    private static List<RawMaterial> raw_materials_used = arrangeParts();
    
    public static List<RawMaterial> arrangeParts()
    {
        List<RawMaterial> raw_materials_used = new List<RawMaterial>();
        
        List<Part> part_list = ExcelDataLoader.PartListFromExcel("/Users/suchacoolguy/Documents/BOM_test.xlsx");
        // sort in descending order
        part_list.Sort((a, b) => b.Length.CompareTo(a.Length));
        // sort in descending order
        length_options_rawMaterial.Sort((a, b) => b.CompareTo(a));
        
        RawMaterial raw_material = null;
        int if_count = 0;
        
        RawMaterial best_of_the_best_combination = null;
        List<RawMaterial> best_combination_list = new List<RawMaterial>();
        
        
        List<int> garra_parts = new List<int>() {2844, 2844};
        raw_material = garra_creator(part_list, garra_parts, 6010);
        raw_materials_used.Add(raw_material);
        
        raw_material = garra_creator(part_list, garra_parts, 6010);
        raw_materials_used.Add(raw_material);
        
        raw_material = garra_creator(part_list, garra_parts, 6010);
        raw_materials_used.Add(raw_material);
        
        raw_material = garra_creator(part_list, garra_parts, 6010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>();
        raw_material = garra_creator(part_list, garra_parts, 6010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {2868, 2844};
        raw_material = garra_creator(part_list, garra_parts, 6010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3393, 3131};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {6669};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {6669};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {6669};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {6669};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3393, 3393};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3394, 3394};
        raw_material = garra_creator(part_list, garra_parts, 7010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3581, 3581};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4843, 2489};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3731, 3731};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3731, 3731};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3731, 3731};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3731, 3731};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3731, 3731};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {1881, 1881, 1881, 1881};
        raw_material = garra_creator(part_list, garra_parts, 8010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {2844, 2624, 2331};
        raw_material = garra_creator(part_list, garra_parts, 8010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {5479, 2331};
        raw_material = garra_creator(part_list, garra_parts, 8010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4682, 4682};
        raw_material = garra_creator(part_list, garra_parts, 9510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3131, 3131, 3131};
        raw_material = garra_creator(part_list, garra_parts, 9510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3131, 3131, 3131};
        raw_material = garra_creator(part_list, garra_parts, 9510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3131, 3131, 3131};
        raw_material = garra_creator(part_list, garra_parts, 9510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3131, 3131, 3131};
        raw_material = garra_creator(part_list, garra_parts, 9510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3131, 3131, 3131};
        raw_material = garra_creator(part_list, garra_parts, 9510);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4832, 4832};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4832, 4832};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4832, 4832};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4832, 4832};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {4843, 4843};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3231, 3231, 3231};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3231, 3231, 3231};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3231, 3231, 3231};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {9705};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {2487, 2487, 2487, 2487};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {2487, 2487, 2487, 2487};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {2487, 2487, 2487, 2487};
        raw_material = garra_creator(part_list, garra_parts, 10010);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {10137};
        raw_material = garra_creator(part_list, garra_parts, 12110);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {10504};
        raw_material = garra_creator(part_list, garra_parts, 12110);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {10604};
        raw_material = garra_creator(part_list, garra_parts, 12110);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {11304};
        raw_material = garra_creator(part_list, garra_parts, 12110);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {12004};
        raw_material = garra_creator(part_list, garra_parts, 12110);
        raw_materials_used.Add(raw_material);
        
        garra_parts = new List<int>() {3731, 3581};
        raw_material = garra_creator(part_list, garra_parts, 7510);
        raw_materials_used.Add(raw_material);

        

        return raw_materials_used;
    }
    
    public static RawMaterial garra_creator(List<Part> part_list, List<int> part_length, int raw_length)
    {
        RawMaterial raw = new RawMaterial(raw_length);
        int trial = part_length.Count;
        
        foreach (var part in part_list)
        {
            foreach (var required_len in part_length)
            {
                if (part.Length == required_len && part.Num > 0 && trial > 0)
                {
                    raw.add_part(part);
                    part.Num--;
                    trial--;
                }   
            }
        }    
        return raw;
    }

    public List<RawMaterial> GetArrangedRawMaterials()
    {
        return raw_materials_used;
    }
}