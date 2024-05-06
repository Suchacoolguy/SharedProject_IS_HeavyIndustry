using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;

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
        var rectangle = this.FindControl<Rectangle>("Rectangle_RawMaterial");
        var rawMaterial = rectangle?.Tag as RawMaterial;
        var data = e.Data as IDataObject;
        var part = data.Get("part") as Part;

        if (rawMaterial != null && part != null)
        {
            // rawMaterial.add_part(part);
            Console.WriteLine("Drop");
            Console.WriteLine(rawMaterial);
        }

        if (rectangle == null)
        {
            Console.WriteLine("Rectangle is null");
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
}