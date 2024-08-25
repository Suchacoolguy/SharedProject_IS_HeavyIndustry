using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
#pragma warning restore CA1822 // Mark members as static
    
    public static DragAndDropViewModel DragAndDropViewModel { get; set; }
    public static BOMDataViewModel BomDataViewModel { get; set; }
    //public static SettingsViewModel SettingsViewModel { get; set; }
    public static string ProjectName { get; set; } = "";
    public static string SelectedKey { get; set; } = "";
    public static Dictionary<string, ObservableCollection<RawMaterial?>> RawMaterialSet { get; set; } 
        = new Dictionary<string, ObservableCollection<RawMaterial?>>();
    
    public static Dictionary<string, ObservableCollection<Part>> TempPartSet { get; set; } 
        = new Dictionary<string, ObservableCollection<Part>>();

    public MainWindowViewModel()
    {
    }
    
    public void DropPartOntoRawMaterial(Part part, RawMaterial rawMaterial)
    {
        // Logic to add the part to the raw material
        rawMaterial.insert_part(part);
    }
    
    public static void ClearTempPartList()
    {
        DragAndDropViewModel.TempPartList.Clear();
    }
    
    public static int CountTempPartList()
    {
        return DragAndDropViewModel.TempPartList.Count;
    }
    
    public static void UpdateRawMaterialSet(ObservableCollection<RawMaterial?> rawMaterialSet, string key)
    {
        if (RawMaterialSet.ContainsKey(key))
        {
            RawMaterialSet[key] = rawMaterialSet;
        }
        else
        {
            RawMaterialSet.TryAdd(key, rawMaterialSet);
        }
    }
    
    public static void UpdateTempPartSet(ObservableCollection<Part> tempPartSet, string key)
    {
        if (TempPartSet.ContainsKey(key))
        {
            TempPartSet[key] = tempPartSet;
        }
        else
        {
            TempPartSet.TryAdd(key, tempPartSet);
        }
    }
}