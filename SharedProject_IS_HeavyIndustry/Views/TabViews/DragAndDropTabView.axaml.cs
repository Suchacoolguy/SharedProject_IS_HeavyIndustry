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
    private string key = "";
    
    public DragAndDropTabView(MainWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        MainWindowViewModel.RawMaterialSet.Clear();
        DataContext = new DragAndDropTabViewModel();
    }

    private void AddDragAndDrop(object? sender, SelectionChangedEventArgs e)
    {
        var selectedMaterial = this.FindControl<ComboBox>("Material")!.SelectedItem?.ToString();
        var selectedDescription = this.FindControl<ComboBox>("Description")!.SelectedItem?.ToString();
        var dockPanel = this.FindControl<Panel>("Parent_DragAndDrop");

        ///////////////////////////////////////////테스트용 코드

        ObservableCollection<Part> parts = null!;
        ObservableCollection<Part> partsOverLength = null!;
        if (selectedMaterial != null && selectedDescription != null)
        {
            key = selectedMaterial + "," + selectedDescription;
            //딕셔너리에 키가 존재하는지 확인 -> 존재하면 한번 DragAndDrop처리 했던것이므로 해당 값 불러와서 DragAndDropView추가 
            if (MainWindowViewModel.RawMaterialSet.TryGetValue(key, out var arrangedRawMaterials))
            {
                dockPanel!.Children.RemoveAll(dockPanel.Children);
                DragAndDropViewModel.ArrangedRawMaterials = arrangedRawMaterials;
                dockPanel.Children.Add(new DragAndDropView(mainWindow));
                return;
            }
            
            parts = GetFilteredParts(selectedMaterial, selectedDescription,
                BOMDataViewModel.PartsForTask);
            partsOverLength = GetFilteredParts(selectedMaterial, selectedDescription,
                BOMDataViewModel.PartsToSeparate);
            
            var service = new ArrangePartsService(new List<Part>(parts), partsOverLength, SettingsViewModel.GetLengthOption(selectedDescription));
            MainWindowViewModel.DragAndDropViewModel =
                new DragAndDropViewModel(service.GetArrangedRawMaterials(), service.GetOverSizeParts());
            

            if (!MainWindowViewModel.RawMaterialSet.ContainsKey(key))
            {
                //MainWindowViewModel.RawMaterialSet[key] = new ObservableCollection<RawMaterial>();
                MainWindowViewModel.RawMaterialSet.TryAdd(key, service.GetArrangedRawMaterials());
            }
        
            
            
            
            if (!dockPanel.Children.Any())
            {
                var dragAndDropView = new DragAndDropView(mainWindow);
                dockPanel.Children.Add(dragAndDropView);
            }
            else
            {
                UpdateRawMaterialToDictionary();
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
    
    private static ObservableCollection<Part> GetFilteredParts(string material, string desc, ObservableCollection<Part> parts)
    {
        var list = new ObservableCollection<Part>();

        foreach (var part in parts)
            if (part.Desc.ToString().Equals(desc) && part.Material.Equals(material))
                list.Add(part);

        return list;
    }

    private void UpdateRawMaterialToDictionary()
    {
        MainWindowViewModel.RawMaterialSet[key] = DragAndDropViewModel.ArrangedRawMaterials;
    }
}