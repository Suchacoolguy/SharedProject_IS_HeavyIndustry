using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class TempPartsView : UserControl
{
    private Point _ghostPosition = new(0,0);
    private readonly Point _mouseOffset = new(-5, -5);
    public TempPartsView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, TempPart_DragOver);
        AddHandler(DragDrop.DropEvent, DragAndDropViewModel.Part_Drop);
        
        Console.WriteLine("Num OverSizeParts: " + DragAndDropViewModel.OverSizeParts.Count);
    }
    
    private void TempPart_DragOver(object sender, DragEventArgs e)
    {
        var data = e.Data.Get("part");
        if (data is not Part part) return;
    }

    private async void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Console.WriteLine("Temp Part PointerPressed is called");
        var part = (sender as Control)?.DataContext as Part;
        
        var data = new DataObject();
        data.Set("part", part);
        
        if (part != null)
            MainWindowViewModel.DragAndDropViewModel.DraggedPart = part;
        
        foreach (Part p in DragAndDropViewModel.TempPartList)
        {
            if (ReferenceEquals(part, p))
            {
                DragAndDropViewModel.TempPartList.Remove(p);
                break;
            }
        }
        var result = await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
    }
}