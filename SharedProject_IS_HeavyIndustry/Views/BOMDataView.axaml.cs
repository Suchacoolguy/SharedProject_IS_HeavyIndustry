using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Services;
using Border = OfficeOpenXml.Style.Border;

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
        RawMaterial rawMaterial = null;

        ItemsControl itemsControl = this.FindControl<ItemsControl>("RawMaterialList");
        for(int i = 0; i < itemsControl.Items.Count; i++)
        {
            RawMaterial temp = itemsControl.Items[i] as RawMaterial;
            foreach (Part p in temp.PartsInside)
            {
                if (p.Equals(part))
                {
                    rawMaterial = temp;      
                    break;
                }
            }
            
            // Console.WriteLine(rawMaterial);
        }
        

        // var rawMaterial = rawMaterialRectangle?.Tag as RawMaterial;

        if (part != null && rawMaterial != null)
        {
            // the part object being dragged
            var data = new DataObject();
            data.Set("part", part);
            data.Set("originalRawMaterial", rawMaterial);
            
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
        var originalRawMaterial = data.Get("originalRawMaterial") as RawMaterial;

        if (originalRawMaterial == null)
        {
            Console.WriteLine("originalRawMaterial is null");    
        }
        
        // Get the RawMaterial object from the sender
        var rawMaterial = (e.Source as Control)?.Tag as RawMaterial;

        if (rawMaterial != null && part != null)
        {
            // rawMaterial.add_part(part);
            // rawMaterial.insert_part(part);
        
            // Update the ArrangedRawMaterials collection in the ViewModel
            var viewModel = DataContext as MainWindowViewModel;
            viewModel?.DragAndDropData.UpdateRawMaterial(originalRawMaterial, rawMaterial, part);
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