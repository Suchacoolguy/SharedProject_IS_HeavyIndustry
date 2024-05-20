using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class DragAndDropViewModel
{
    public static ObservableCollection<RawMaterial> ArrangedRawMaterials { get; set; }
    public RawMaterial CurrentRawMaterial { get; set; }
    
    public DragAndDropViewModel(ObservableCollection<RawMaterial> arranged_raw_materials)
    {
        ArrangedRawMaterials = new ObservableCollection<RawMaterial>(arranged_raw_materials);
    }
    
    public void UpdateRawMaterial(RawMaterial from, RawMaterial to, Part part)
    {
        int index_from = ArrangedRawMaterials.IndexOf(from);
        int index_to = ArrangedRawMaterials.IndexOf(to);
        int index_part = ArrangedRawMaterials[index_from].PartsInside.IndexOf(part);
        
        if (index_to > -1 && index_to < ArrangedRawMaterials.Count)
        {
            ArrangedRawMaterials[index_to].insert_part(part);
            ArrangedRawMaterials[index_from].PartsInside.RemoveAt(index_part);
            // if (ArrangedRawMaterials[index_from].PartsInside.Count == 0)
            // {
            //     ArrangedRawMaterials.RemoveAt(index_from);
            // }
        }
        else
        {
            // Console.WriteLine("got into else statement.");
        }

        // If the user drops the part object into an area that doesn't contain any other objects
        if (to == null)
        {
            // find the best size of raw material to insert the part
            List<int> lengthOptions = GetLengthOptionsRawMaterial();
            int bestLength = Int32.MaxValue;
            foreach (var len in lengthOptions)
            {
                if (len >= part.Length && len < bestLength)
                {
                    bestLength = len;
                }
            }
            
            // insert the new raw material beyo
            RawMaterial newRawMaterial = new RawMaterial(bestLength);
            ArrangedRawMaterials.Insert(index_from + 1, newRawMaterial);
        }
    }
    
    public List<int> GetLengthOptionsRawMaterial()
    {
        return ArrangePartsService.GetLengthOptionsRawMaterial();
    }
    
    
    
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