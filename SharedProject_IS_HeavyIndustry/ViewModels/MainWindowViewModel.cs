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

    public MainWindowViewModel()
    {
        var service = new ArrangePartsService();
        DragAndDropData = new DragAndDropViewModel(service.GetArrangedRawMaterials(), service.GetOverSizeParts());
        
        var bomService = new BOMDataService();
        BOMData = new BOMDataViewModel(bomService.GetPartList());
    }
    public BOMDataViewModel BOMData { get; }
    public DragAndDropViewModel DragAndDropData { get; }
    
    public void DropPartOntoRawMaterial(Part part, RawMaterial rawMaterial)
    {
        // Logic to add the part to the raw material
        rawMaterial.insert_part(part);
    }
}