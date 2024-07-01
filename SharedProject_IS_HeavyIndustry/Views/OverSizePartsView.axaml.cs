using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class OverSizePartsView : UserControl
{
    public OverSizePartsView()
    {
        InitializeComponent();
        // AddHandler(DragDrop.DragOverEvent, RawMaterial_DragOver);
        // AddHandler(DragDrop.);
        AddHandler(DragDrop.DropEvent, DragAndDropViewModel.Part_Drop);

        Console.WriteLine(":::OVERSIZE PARTS:::");
        foreach (var part in DragAndDropViewModel.OverSizeParts)
        {
            
            Console.WriteLine(part);
        }
    }

    private void Part__PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var part = (sender as Control)?.DataContext as Part;
        
        if (part != null)
        {
            // the part object being dragged
            var data = new DataObject();
            data.Set("part", part);
            
            DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
        }
    }
}