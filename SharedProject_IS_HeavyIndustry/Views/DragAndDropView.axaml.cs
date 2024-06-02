using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class DragAndDropView : TabView
{
    public DragAndDropView(StartWindow mainWindow)
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, DragAndDropViewModel.RawMaterial_DragOver);
        AddHandler(DragDrop.DropEvent, DragAndDropViewModel.RawMaterial_Drop);
    }

    private void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var part = (sender as Control)?.DataContext as Part;
        RawMaterial originalRawMaterial = null;
        
        foreach (RawMaterial raw in DragAndDropViewModel.ArrangedRawMaterials)
        {
            foreach (Part p in raw.PartsInside)
            {
                if (ReferenceEquals(part, p))
                {
                    originalRawMaterial = raw;
                    break;
                }
            }   
        }

        if (part != null && originalRawMaterial != null)
        {
            // the part object being dragged
            var data = new DataObject();
            data.Set("part", part);
            data.Set("originalRawMaterial", originalRawMaterial);
            
            DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
        }
        
    }
    
    
    
    
    
}