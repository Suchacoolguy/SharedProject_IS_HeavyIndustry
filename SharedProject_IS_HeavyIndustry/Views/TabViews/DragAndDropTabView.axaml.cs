using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class DragAndDropTabView : TabView
{
    private MainWindow mainWindow;
    
    public DragAndDropTabView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new DragAndDropTabViewModel();
    }

    private void AddDragAndDrop(object? sender, SelectionChangedEventArgs e)
    {
        var selectedType = this.FindControl<ComboBox>("Type")!.SelectedItem?.ToString();
        var selectedSize = this.FindControl<ComboBox>("Size")!.SelectedItem?.ToString();
        var dockPanel = this.FindControl<Panel>("Parent_DragAndDrop");
        string arrangementType = "Min Scrap";

        ///////////////////////////////////////////테스트용 코드

        ObservableCollection<Part> parts = null!;
        ObservableCollection<Part> partsOverLength = null!;
        if (selectedType != null && selectedSize != null)
        {
            parts = DragAndDropTabViewModel.FindPartsByDescription(new Description(selectedType, selectedSize),
                BOMDataViewModel.PartsForTask);
            partsOverLength = DragAndDropTabViewModel.FindPartsByDescription(new Description(selectedType, selectedSize),
                BOMDataViewModel.PartsToSeparate);
            
            var service = new ArrangePartsService(new List<Part>(parts), partsOverLength, arrangementType);
            MainWindowViewModel.DragAndDropViewModel =
                new DragAndDropViewModel(service.GetArrangedRawMaterials(), service.GetOverSizeParts());
            
            var key = selectedType + selectedSize;
            if (!MainWindowViewModel.RawMaterialSet.ContainsKey(key))
            {
                MainWindowViewModel.RawMaterialSet[key] = new ObservableCollection<RawMaterial>();
                MainWindowViewModel.RawMaterialSet[key].Add(service.GetArrangedRawMaterials());
            }
            
            
            if (!dockPanel.Children.Any())
            {
                var dragAndDropView = new DragAndDropView(mainWindow);
                dockPanel.Children.Add(dragAndDropView);
            }
            else
            {
                dockPanel.Children.RemoveAll(dockPanel.Children);
                
                var dragAndDropView = new DragAndDropView(mainWindow);
                dockPanel.Children.Add(dragAndDropView);
            }
        }
        else
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("알림", "Please select type and size", ButtonEnum.Ok);
            box.ShowAsync();
        }
    }
}