using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class DNDTabView : TabView
{
    private StartWindow mainWindow;
    
    public DNDTabView(StartWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new DNDTabViewModel();
    }

    private void AddDragAndDrop(object? sender, SelectionChangedEventArgs e)
    {
        var selectedType = this.FindControl<ComboBox>("Type")!.SelectedItem?.ToString();
        var selectedSize = this.FindControl<ComboBox>("Size")!.SelectedItem?.ToString();
        var dockPanel = this.FindControl<Panel>("Parent_DragAndDrop");

        ///////////////////////////////////////////테스트용 코드

        ObservableCollection<Part> parts = null!;
        ObservableCollection<Part> partsOverLength = null!;
        if (selectedType != null && selectedSize != null)
        {
            parts = DNDTabViewModel.FindPartsByDescription(new Description(selectedType, selectedSize),
                BOMDataViewModel.PartsForTask);
            partsOverLength = DNDTabViewModel.FindPartsByDescription(new Description(selectedType, selectedSize),
                BOMDataViewModel.PartsToSeparate);
            
            var service = new ArrangePartsService(new List<Part>(parts), partsOverLength);
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
            MessageBox.Show(mainWindow, "선택된 파트가 없습니다.", "알림", MessageBox.MessageBoxButtons.YesNoCancel);
        }
    }
}