using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class DragAndDropView : TabView
{
    public DragAndDropView(StartWindow mainWindow)
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, DragAndDropViewModel.RawMaterial_DragOver);
        AddHandler(DragDrop.DropEvent, DragAndDropViewModel.RawMaterial_Drop);
        
        // Add the OverSizePartsView when the PartsForSeparate list in the WorkManager is not empty.
        if (DragAndDropViewModel.GetOverSizeParts().Count > 0)
        {
            var entireGrid = this.FindControl<Grid>("EntireGrid");
            // Create a new instance of OverSizePartsView
            var overSizePartsView = new OverSizePartsView();

            // Set the Grid.Column property of the OverSizePartsView
            Grid.SetColumn(overSizePartsView, 1);

            // Add the OverSizePartsView to the Grid
            entireGrid.Children.Add(overSizePartsView);
        }
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