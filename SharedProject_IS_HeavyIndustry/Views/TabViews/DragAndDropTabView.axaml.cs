using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
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
        // MainWindowViewModel.RawMaterialSet.Clear(); // ?? 이게 필요한가 + 파트 사라지는 오류 용의자 후보 1번
        DataContext = new DragAndDropTabViewModel();
    }
    private void AddDragAndDrop(object? sender, SelectionChangedEventArgs e)
    {
        var selectedMaterial = this.FindControl<ComboBox>("Material")!.SelectedItem?.ToString();
        var selectedDescription = this.FindControl<ComboBox>("Description")!.SelectedItem?.ToString();
        var dockPanel = this.FindControl<Panel>("Parent_DragAndDrop");
        
        foreach (KeyValuePair<string, ObservableCollection<Part>> item in MainWindowViewModel.TempPartSet)
        {
            // Printing the key and value to the console
            Console.WriteLine($"Key: {item.Key}, Value: {item.Value.Count}");
        }
        
        if (selectedMaterial != null && selectedDescription != null)
        {
            // _key = selectedMaterial + "," + selectedDescription;
            MainWindowViewModel.SelectedKey = selectedMaterial + "," + selectedDescription;
            Console.WriteLine("현재 키 : " + MainWindowViewModel.SelectedKey);
            
            //딕셔너리에 키가 존재하는지 확인 -> 존재하면 한번 DragAndDrop처리 했던것이므로 해당 값 불러와서 DragAndDropView추가 
            if (MainWindowViewModel.RawMaterialSet.TryGetValue(MainWindowViewModel.SelectedKey, out var arrangedRawMaterials))  // 여기!!
            {
                Console.WriteLine("으하하하으하하하으하하하으하하하으하하하으하하하으하하하: " + arrangedRawMaterials.Count);
                dockPanel!.Children.RemoveAll(dockPanel.Children);
                DragAndDropViewModel.ArrangedRawMaterials = arrangedRawMaterials;
                
                if (MainWindowViewModel.TempPartSet.TryGetValue(MainWindowViewModel.SelectedKey, out var overSizedParts))  // 여기!!
                    DragAndDropViewModel.PartsCanNotBeArranged = overSizedParts;
                
                ArrangePartsService._lengthOptionsRawMaterial = SettingsViewModel.GetLengthOption(selectedDescription);
                
                dockPanel.Children.Add(new DragAndDropView(mainWindow));
                return;
            }
            
            // 처음 선택한 규격인 경우 (이미 배치한 파트가 없는 경우)
            BuildDragAndDropData(selectedMaterial, selectedDescription);
            
            if (dockPanel!.Children.Any())
            {
                dockPanel.Children.RemoveAll(dockPanel.Children);
            }
            
            var dragAndDropView = new DragAndDropView(mainWindow);
            dockPanel.Children.Add(dragAndDropView);
        }
        else
            MessageService.Send("재질과 규격을 선택하세요");
    }

    private void BuildDragAndDropData(string selectedMaterial, string selectedDescription)
    {
        ObservableCollection<Part> parts = null!;
        ObservableCollection<Part> partsOverLength = null!;
        
        parts = GetFilteredParts(selectedMaterial, selectedDescription,
            BOMDataViewModel.PartsForTask);
        partsOverLength = GetFilteredParts(selectedMaterial, selectedDescription,
            BOMDataViewModel.PartsToSeparate);
        
        // Console.WriteLine("-------------------" + MainWindowViewModel.DragAndDropViewModel.TempPartList.Count());
        Console.WriteLine("DND View Model - Temp Part List:" + DragAndDropViewModel.PartsCanNotBeArranged.Count);
        
        Console.WriteLine(selectedDescription);
        foreach (var VARIABLE in partsOverLength)
        {
            Console.WriteLine(":::::" + VARIABLE + ":::::");
            Console.WriteLine(VARIABLE.NeedSeparate);
        }
        
        var service = new ArrangePartsService(new List<Part>(parts), partsOverLength,
            SettingsViewModel.GetLengthOption(selectedDescription));
        MainWindowViewModel.DragAndDropViewModel =
            new DragAndDropViewModel(service.GetArrangedRawMaterials(), service.getPartsCanNotBeArranged());


        if (!MainWindowViewModel.RawMaterialSet.ContainsKey(MainWindowViewModel.SelectedKey))
        {
            MainWindowViewModel.RawMaterialSet.TryAdd(MainWindowViewModel.SelectedKey, service.GetArrangedRawMaterials());
        }
        
        if (!MainWindowViewModel.TempPartSet.ContainsKey(MainWindowViewModel.SelectedKey))
        {
            MainWindowViewModel.TempPartSet.TryAdd(MainWindowViewModel.SelectedKey, service.getPartsCanNotBeArranged());
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
}