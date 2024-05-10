using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class DragAndDropViewModel
{
    public DragAndDropViewModel(List<RawMaterial> arranged_raw_materials)
    {
        ArrangedRawMaterials = new ObservableCollection<RawMaterial>(arranged_raw_materials);
    }
    
    public void UpdateRawMaterial(RawMaterial from, RawMaterial to, Part part)
    {
        int index_from = ArrangedRawMaterials.IndexOf(from);
        int index_to = ArrangedRawMaterials.IndexOf(to);
        int index_part = ArrangedRawMaterials[index_from].PartsInside.IndexOf(part);
        
        Console.WriteLine("index from:" + index_from);
        Console.WriteLine("index to:" + index_to);
        
        // Console.WriteLine("Part Index: " + index_part);
        // Console.WriteLine("Raw Count:" + ArrangedRawMaterials.Count);
        if (index_to > -1 && index_to < ArrangedRawMaterials.Count)
        {
            ArrangedRawMaterials[index_to].insert_part(part);
            ArrangedRawMaterials[index_from].PartsInside.RemoveAt(index_part);
            Console.WriteLine(ArrangedRawMaterials[index_to]);
        }
        else
        {
            Console.WriteLine("got into else statement.");
        }
    }
    
    public static ObservableCollection<RawMaterial> ArrangedRawMaterials { get; set; }
    public RawMaterial CurrentRawMaterial { get; set; }
}

// public BOMDataViewModel(List<Part> parts)
// {
//     ListParts = new ObservableCollection<Part>(parts);
//     Console.WriteLine(ListParts.GetType());
//     foreach (var part in ListParts)
//     {
//         Console.WriteLine("Length:" + part.Length);
//     }
// }
//     
// public ObservableCollection<Part> ListParts { get; }