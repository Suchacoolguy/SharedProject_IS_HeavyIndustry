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
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DynamicData;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class DragAndDropView : TabView
{
    private Point _ghostPosition = new(0,0);
    private readonly Point _mouseOffset = new(-5, -5);
    private bool? _bomSortCheck = false, _scrapSortCheck = false;
    public static MenuItem? bomAscending;
    public static MenuItem? bomDescending;
    public static MenuItem? scrapAscending;
    public static MenuItem? scrapDescending;

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
        
        
        bomAscending = this.FindControl<MenuItem>("BomAscending")!;
        bomDescending = this.FindControl<MenuItem>("BomDescending")!;
        scrapAscending = this.FindControl<MenuItem>("ScrapAscending")!;
        scrapDescending = this.FindControl<MenuItem>("ScrapDescending")!;
    }
    
    private void RawMaterial_DragOver(object? sender, DragEventArgs e)
    {
        var currentPosition = e.GetPosition(EntireGrid);

        var offsetX = currentPosition.X - _ghostPosition.X;
        var offsetY = currentPosition.Y - _ghostPosition.Y;

        GhostItem.RenderTransform = new TranslateTransform(offsetX, offsetY);
        
        var data = e.Data.Get("part");
        if (data is not Part part) return;
    }

    private async void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var part = (sender as Control)?.DataContext as Part;
        var PartRectangle = (sender as Border);
        RawMaterial? originalRawMaterial = null;
        
        if (part != null)
            MainWindowViewModel.DragAndDropViewModel.DraggedPart = part;
        
        foreach (RawMaterial? raw in DragAndDropViewModel.ArrangedRawMaterials)
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
            if (PartRectangle != null) 
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
    
    private void SortByRawMaterialLength_Ascending()
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => x.Length.CompareTo(y.Length));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            DragAndDropViewModel.ArrangedRawMaterials.AddRange(rawList);
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials);
        }
    }
    
    private void SortByRawMaterialLength_Descending()
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => y.Length.CompareTo(x.Length));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            DragAndDropViewModel.ArrangedRawMaterials.AddRange(rawList);
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials);

        }
    }
    
    private void SortByScrapAscending()
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => x.RemainingLength.CompareTo(y.RemainingLength));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            DragAndDropViewModel.ArrangedRawMaterials.AddRange(rawList);
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials);
        }
    }
    
    private void SortByScrapDescending()
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials != null)
        {
            List<RawMaterial> rawList = rawMaterials.ToList();
            rawList.Sort((x, y) => y.RemainingLength.CompareTo(x.RemainingLength));

            DragAndDropViewModel.ArrangedRawMaterials.Clear();
            DragAndDropViewModel.ArrangedRawMaterials.AddRange(rawList);
            MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials);
        }
    }
    
    public static void InitializeSortOption()
    {
        if (bomAscending != null) bomAscending.Icon = null;
        if (bomDescending != null) bomDescending.Icon = null;
        if (scrapAscending != null) scrapAscending.Icon = null;
        if (scrapDescending != null) scrapDescending.Icon = null;
    }
    
    private void Sort(object? sender, RoutedEventArgs e)
    {
        MenuCheck((sender as MenuItem)!);
        
        if ((bomAscending?.Icon != null || bomDescending?.Icon != null) &&
            (scrapAscending?.Icon != null || scrapDescending?.Icon != null))
            SortTwoCondition();
        else
        {
            if(bomAscending?.Icon != null)
                SortByRawMaterialLength_Ascending();
            else if(bomDescending?.Icon != null)
                SortByRawMaterialLength_Descending();
            else if(scrapAscending?.Icon != null)
                SortByScrapAscending();
            else if(scrapDescending?.Icon != null)
                SortByScrapDescending();
        }
    }
    
    private void MenuCheck(MenuItem clickedItem)
    {
        if (clickedItem!.Icon != null)
            clickedItem.Icon = null;
        else
        {
            clickedItem.Icon = new TextBlock { Text = "\u2714" };
            if (clickedItem!.Name!.Equals("BomAscending") || clickedItem!.Name!.Equals("BomDescending"))
            {
                if (bomAscending.IsSelected)
                    bomDescending.Icon = null;
                else
                    bomAscending.Icon = null;
            }
            else if (clickedItem!.Name!.Equals("ScrapAscending") || clickedItem!.Name!.Equals("ScrapDescending"))
            {
                if (scrapAscending.IsSelected)
                    scrapDescending.Icon = null;
                else
                    scrapAscending.Icon = null;
            }
        }
    }

    private void SortTwoCondition()
    {
        MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var rawMaterials);
        if (rawMaterials == null) return;
        IOrderedEnumerable<RawMaterial> tempList = null!;
        List<RawMaterial> rawList = null!;
        tempList = bomAscending?.Icon != null ? 
            rawMaterials.OrderBy(x => x.Length) : rawMaterials.OrderByDescending(x => x.Length);

        rawList = scrapAscending?.Icon != null ? 
            tempList.ThenBy(x => x.RemainingLength).ToList() : tempList.ThenByDescending(x => x.RemainingLength).ToList();
        
        DragAndDropViewModel.ArrangedRawMaterials.Clear();
        DragAndDropViewModel.ArrangedRawMaterials.AddRange(rawList);
        MainWindowViewModel.UpdateRawMaterialSet(DragAndDropViewModel.ArrangedRawMaterials);
    }
}