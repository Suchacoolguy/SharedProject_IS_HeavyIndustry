using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class BOMDataView : UserControl
{
    public BOMDataView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, RawMaterial_DragOver);
        AddHandler(DragDrop.DropEvent, RawMaterial_Drop);
    }

    private void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var part = (sender as Control)?.DataContext as Part;
        if (part != null)
        {
            // the part object being dragged
            var data = new DataObject();
            data.Set("part", part);
            
            DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
            Console.WriteLine("pointer pressed");
        }
        
    }

    private void RawMaterial_DragOver(object sender, DragEventArgs e)
    {
        e.DragEffects = e.DragEffects & DragDropEffects.Move;
        e.Handled = true;
    }

    private void RawMaterial_Drop(object sender, DragEventArgs e)
    {
        // the part object being dragged
        var data = e.Data as IDataObject;
        if (data == null)
        {
            Console.WriteLine("Data is null");
            return;
        }

        var part = data.Get("part") as Part;

        // Get the RawMaterial object from the sender
        var rawMaterial = (e.Source as Control)?.Tag as RawMaterial;

        if (rawMaterial != null && part != null)
        {
            // rawMaterial.add_part(part);
            rawMaterial.insert_part(part);
        }

        if (part != null)
        {
            Console.WriteLine(part);
        }
        if (rawMaterial == null)
        {
            Console.WriteLine("RawMaterial is null");
        }
    }
    
    // private void RawMaterial_PointerReleased(object sender, PointerReleasedEventArgs e)
    // {
    //     var rawMaterial = (sender as Control)?.Tag as RawMaterial;
    //     if (rawMaterial != null)
    //     {
    //         Console.WriteLine(rawMaterial);
    //     }
    // }
}