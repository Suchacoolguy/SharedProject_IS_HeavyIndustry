using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class DragAndDropViewModel
{
    public static ObservableCollection<RawMaterial> ArrangedRawMaterials { get; set; }
    public static ObservableCollection<Part> OverSizeParts { get; set; }
    public RawMaterial CurrentRawMaterial { get; set; }

    public DragAndDropViewModel(ObservableCollection<RawMaterial> arrangedRawMaterials,
        ObservableCollection<Part> overSizeParts)
    {
        ArrangedRawMaterials = new ObservableCollection<RawMaterial>(arrangedRawMaterials);
        OverSizeParts = new ObservableCollection<Part>(overSizeParts);
    }

    public static void UpdateRawMaterial(RawMaterial from, RawMaterial to, Part part)
    {
        int index_from = 0;
        int index_to = 0;
        int index_part = 0;
        int len_part = 0;
        
        if (from != null)
            index_from = ArrangedRawMaterials.IndexOf(from);
        if (to != null)
            index_to = ArrangedRawMaterials.IndexOf(to);
        if (part != null)
        {
            index_part = ArrangedRawMaterials[index_from].PartsInside.IndexOf(part);
            len_part = part.Length;
        }
            

        if (from != null && to != null && part != null)
        {
            if (index_to > -1 && index_to < ArrangedRawMaterials.Count)
            {
                ArrangedRawMaterials[index_to].insert_part(part);
                ArrangedRawMaterials[index_from].remove_part_at(index_part);
                if (ArrangedRawMaterials[index_from].PartsInside.Count == 0)
                {
                    ArrangedRawMaterials.RemoveAt(index_from);
                }
            }

            Console.WriteLine("UpdateRawMaterial - from: not null, to: not null, part: not null");
        }
        else if (to == null && part != null)
        {
            // If the user drops the part object into an area that doesn't contain any other objects
            // find the best size of raw material to insert the part

            Console.WriteLine("UpdateRawMaterial - from: null, to: null, part: not null");
            Console.WriteLine("여긴 뭐하는 곳인가");
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
            ArrangedRawMaterials.Insert(ArrangedRawMaterials.Count, newRawMaterial);
            ArrangedRawMaterials[index_from].remove_part_at(index_part);
            
            if (ArrangedRawMaterials[index_from].PartsInside.Count == 0)
            {
                ArrangedRawMaterials.RemoveAt(index_from);
            }
        }
        else if (from == null && to != null && part != null)
        {
            // for the case where the user drags a part from the overSizeParts collection
            Console.WriteLine("UpdateRawMaterial - from: null, to: not null, part: not null");
            
            ArrangedRawMaterials[index_to].insert_part(part);
            OverSizeParts.Remove(part);
        }
        else if (from != null && to == null && part != null)
        {
            // when the user drags a part from a raw material to empty space
            OverSizeParts.Add(part);
            ArrangedRawMaterials[index_from].remove_part_at(index_part);
            Console.WriteLine("UpdateRawMaterial - from: not null, to: null, part: not null");
        }
        else if (from == null && to == null && part != null)
        {
            Console.WriteLine("여기다 여기!");
        }
    }
    
    public static void RawMaterial_DragOver(object sender, DragEventArgs e)
    {
        e.DragEffects = e.DragEffects & DragDropEffects.Move;
        e.Handled = true;
    }
    
    public static void RawMaterial_Drop(object sender, DragEventArgs e)
    {
        Console.WriteLine("RawMaterial_Drop Executed.");
        
        // the part object being dragged
        var data = e.Data as IDataObject;
        if (data == null)
        {
            Console.WriteLine("Data is null");
            return;
        }

        var part = data.Get("part") as Part;
        var rawMaterialFrom = data.Get("originalRawMaterial") as RawMaterial;

        if (rawMaterialFrom == null)
        {
            Console.WriteLine("RawMaterial_Drop - from is null");    
        }
        
        // Get the RawMaterial object from the sender
        var rawMaterialTo = (e.Source as Control)?.Tag as RawMaterial;
        if (rawMaterialTo == null)
        {
            Console.WriteLine("RawMaterial_Drop - to is null");   
        }

        if (part == null)
        {
            Console.WriteLine("RawMaterial_Drop - part is null");
        }
        
        // var viewModel = DataContext as MainWindowViewModel;

        if (rawMaterialFrom != null && rawMaterialTo != null && part != null)
        {
            // Update the ArrangedRawMaterials collection in the ViewModel
            
            UpdateRawMaterial(rawMaterialFrom, rawMaterialTo, part);
            Console.WriteLine("RawMaterial_Drop - from: not null, to: not null, part: not null");
        }
        else if (rawMaterialFrom == null && rawMaterialTo != null && part != null)
        {
            UpdateRawMaterial(null, rawMaterialTo, part);
            Console.WriteLine("RawMaterial_Drop - from: null, to: not null, part: not null");
        }
        else if (rawMaterialFrom != null && rawMaterialTo == null && part != null)
        {
            UpdateRawMaterial(rawMaterialFrom, null, part);
            Console.WriteLine("RawMaterial_Drop - to: null, part: not null");
        }
        else if (rawMaterialFrom == null && rawMaterialTo == null && part != null)
        {
            int index_from = 0;
            int index_to = 0;
            int index_part = 0;
            int len_part = 0;
        
            if (rawMaterialFrom != null)
                index_from = ArrangedRawMaterials.IndexOf(rawMaterialFrom);
            if (rawMaterialTo != null)
                index_to = ArrangedRawMaterials.IndexOf(rawMaterialTo);
            if (part != null)
            {
                index_part = OverSizeParts.IndexOf(part);
                len_part = part.Length;
            }
            
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
            ArrangedRawMaterials.Insert(ArrangedRawMaterials.Count, newRawMaterial);
            OverSizeParts.Remove(part);
        }

        if (part != null)
        {
            Console.WriteLine(part);
        }
        if (rawMaterialTo == null)
        {
            Console.WriteLine("RawMaterial_Drop - RawMaterial is null");
        }
    }
    
    public static void Part_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        Console.WriteLine("Part_Drop Executed.");
        // var part = (sender as Control)?.DataContext as Part;
        var data = e.Data as IDataObject;
        if (data == null)
        {
            return;
        }
        var part = data.Get("part") as Part;
        var rawMaterialFrom = data.Get("originalRawMaterial") as RawMaterial;

        if (rawMaterialFrom != null && part != null)
        {
            // var viewModel = DataContext as MainWindowViewModel;
            // viewModel?.DropPartOntoRawMaterial(part, rawMaterial);
            UpdateRawMaterial(rawMaterialFrom, null, part);
        }
    }

    public static List<int> GetLengthOptionsRawMaterial()
    {
        return ArrangePartsService.GetLengthOptionsRawMaterial();
    }

    public static ObservableCollection<Part> GetOverSizeParts()
    {
        return OverSizeParts;
    }
}