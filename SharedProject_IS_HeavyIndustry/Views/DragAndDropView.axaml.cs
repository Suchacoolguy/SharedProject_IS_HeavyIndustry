using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Services;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class DragAndDropView : TabView
{
    private Point _ghostPosition = new(0,0);
    private readonly Point _mouseOffset = new(-5, -5);
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        GhostItem.IsVisible = false;
        base.OnLoaded(e);
    }
    
    public DragAndDropView(MainWindow mainWindow)
    {
        InitializeComponent();
        
        AddHandler(DragDrop.DragOverEvent, RawMaterial_DragOver);
        AddHandler(DragDrop.DropEvent, DragAndDropViewModel.RawMaterial_Drop);
        
        Console.WriteLine("yoyoyoyoyoyooyoyoyo");
        var entireGrid = this.FindControl<Grid>("EntireGrid");
        // Create a new instance of OverSizePartsView
        var overSizePartsView = new OverSizePartsView();
        
        // Set the Grid.Column property of the OverSizePartsView
        Grid.SetColumn(overSizePartsView, 1);
        
        // Add the OverSizePartsView to the Grid
        entireGrid.Children.Add(overSizePartsView);
        
    }
    
    private void RawMaterial_DragOver(object sender, DragEventArgs e)
    {
        var currentPosition = e.GetPosition(MyStackPanel);

        var offsetX = currentPosition.X - _ghostPosition.X;
        var offsetY = currentPosition.Y - _ghostPosition.Y;

        GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);

        // set drag cursor icon
        // e.DragEffects = DragDropEffects.Move;
        // if (DataContext is not DragAndDropViewModel vm) return;
        var data = e.Data.Get("part");
        if (data is not Part part) return;
    }

    private async void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var partRectangle = sender as Rectangle;
        var part = (sender as Control)?.DataContext as Part;
        RawMaterial originalRawMaterial = null;
        
        if (part != null)
            MainWindowViewModel.DragAndDropViewModel.DraggedPart = part;
        
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
            
            var ghostPos = GhostItem.Bounds.Position;
            _ghostPosition = new Point(ghostPos.X + _mouseOffset.X, ghostPos.Y + _mouseOffset.Y);

            var mousePos = e.GetPosition(MyStackPanel);
            var offsetX = mousePos.X - ghostPos.X;
            var offsetY = mousePos.Y - ghostPos.Y + _mouseOffset.X;
            GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);
            
            GhostItem.IsVisible = true;

            // Start the drag operation
            var result = await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
            GhostItem.IsVisible = false;
            
            var rawMaterialTo = (e.Source as Control)?.Tag as RawMaterial;
        }
        
    }
    
    
    
    
    
}