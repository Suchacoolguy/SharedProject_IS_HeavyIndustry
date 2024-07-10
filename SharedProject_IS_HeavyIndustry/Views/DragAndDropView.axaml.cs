using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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
using DocumentFormat.OpenXml.Office2010.ExcelAc;

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
        
        // var entireGrid = this.FindControl<Grid>("EntireGrid");
        // // Create a new instance of OverSizePartsView
        // var overSizePartsView = new TempPartsView();
        //
        // // Set the Grid.Column property of the OverSizePartsView
        // Grid.SetColumn(overSizePartsView, 1);
        //
        // // Add the OverSizePartsView to the Grid
        // entireGrid.Children.Add(overSizePartsView);
        
        AddHandler(DragDrop.DragOverEvent, RawMaterial_DragOver);
        AddHandler(DragDrop.DropEvent, DragAndDropViewModel.RawMaterial_Drop);
        
    }
    
    private void RawMaterial_DragOver(object sender, DragEventArgs e)
    {
        var currentPosition = e.GetPosition(EntireGrid);

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
        var part = (sender as Control)?.DataContext as Part;
        var PartRectangle = (sender as Rectangle);
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
            data.Set("partRectangle", PartRectangle);
            
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
        }
        
    }
    
    private void SortByRawMaterialLength_Ascending(object? sender, RoutedEventArgs e)
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => x.Length.CompareTo(y.Length));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            foreach (var raw in rawList)
            {
                DragAndDropViewModel.ArrangedRawMaterials.Add(raw);
            }
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials, MainWindowViewModel.SelectedKey);
        }
    }
    
    private void SortByRawMaterialLength_Descending(object? sender, RoutedEventArgs e)
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => y.Length.CompareTo(x.Length));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            foreach (var raw in rawList)
            {
                DragAndDropViewModel.ArrangedRawMaterials.Add(raw);
            }
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials, MainWindowViewModel.SelectedKey);
        }
    }
    
    private void SortByScrapAscending(object? sender, RoutedEventArgs e)
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => x.RemainingLength.CompareTo(y.RemainingLength));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            foreach (var raw in rawList)
            {
                DragAndDropViewModel.ArrangedRawMaterials.Add(raw);
            }
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials, MainWindowViewModel.SelectedKey);
        }
    }
    
    private void SortByScrapDescending(object? sender, RoutedEventArgs e)
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => y.RemainingLength.CompareTo(x.RemainingLength));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            foreach (var raw in rawList)
            {
                DragAndDropViewModel.ArrangedRawMaterials.Add(raw);
            }
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials, MainWindowViewModel.SelectedKey);
        }
    }
}