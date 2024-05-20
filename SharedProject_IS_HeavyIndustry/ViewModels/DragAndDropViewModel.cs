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
        
        if (from != null && to != null && part != null)
        {
            if (index_to > -1 && index_to < ArrangedRawMaterials.Count)
            {
                ArrangedRawMaterials[index_to].insert_part(part);
                ArrangedRawMaterials[index_from].PartsInside.RemoveAt(index_part);
                if (ArrangedRawMaterials[index_from].PartsInside.Count == 0)
                {
                    ArrangedRawMaterials.RemoveAt(index_from);
                }
            }
            else
            {
                // Console.WriteLine("got into else statement.");
            }
            Console.WriteLine("if statement.");
        }
        else if(to == null && part != null)
        {   // If the user drops the part object into an area that doesn't contain any other objects
            // find the best size of raw material to insert the part
            
            Console.WriteLine("else if statement.");
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
            newRawMaterial.insert_part(part);
            ArrangedRawMaterials.Insert(index_from + 1, newRawMaterial);
            ArrangedRawMaterials[index_from].PartsInside.RemoveAt(index_part);
            if (ArrangedRawMaterials[index_from].PartsInside.Count == 0)
            {
                ArrangedRawMaterials.RemoveAt(index_from);
            }
        }
    }
    
    public List<int> GetLengthOptionsRawMaterial()
    {
        return ArrangePartsService.GetLengthOptionsRawMaterial();
    }
    
    
    
}