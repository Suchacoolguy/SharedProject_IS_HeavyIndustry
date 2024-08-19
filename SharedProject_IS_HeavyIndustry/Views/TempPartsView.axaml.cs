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
        AddHandler(DragDrop.DropEvent, Part_Drop);
    }
    
    private void TempPart_DragOver(object sender, DragEventArgs e)
    {
        var data = e.Data.Get("temp part");
        if (data is not Part part) return;
    }
    
    public void Part_Drop(object sender, DragEventArgs e)
    {
        DragAndDropView.InitializeSortOption();
        
        // var part = (sender as Control)?.DataContext as Part;
        var data = e.Data as IDataObject;
        if (data == null)
        {
            return;
        }
        var part = data.Get("part") as Part;
        var rawMaterialFrom = data.Get("originalRawMaterial") as RawMaterial;

        if (part != null)
        {
            if (!DragAndDropViewModel.TempPartList.Contains(part) && rawMaterialFrom != null)
            {
                DragAndDropViewModel.TempPartList.Add(part);
                MainWindowViewModel.TempPartSet[MainWindowViewModel.SelectedKey] = DragAndDropViewModel.TempPartList;
                
                rawMaterialFrom.removePart(part);
                if (rawMaterialFrom.PartsInside.Count == 0)
                {
                    DragAndDropViewModel.ArrangedRawMaterials.Remove(rawMaterialFrom);
                }
            }
        }
        
        
        // 여기가 오른쪽 패널에 드랍되고 나서 실행되는 부분
    }

    private async void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Console.WriteLine("Temp Part PointerPressed is called");
        var part = (sender as Control)?.DataContext as Part;

        var data = new DataObject();
        data.Set("temp part", part);
        
        try
        {
            var result = await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
            Console.WriteLine($"DragDrop result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DragDrop operation failed: {ex.Message}");
        }
    }
}