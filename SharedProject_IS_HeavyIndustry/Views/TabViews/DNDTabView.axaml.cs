using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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

        ObservableCollection<Part> parts = null;
        ObservableCollection<Part> partsOverLength = null;
        if (selectedType != null && selectedSize != null)
        {
            parts = WorkManager.FindPartsByDescription(new Description(selectedType, selectedSize),
                WorkManager.PartsForTask);
            partsOverLength = WorkManager.FindPartsByDescription(new Description(selectedType, selectedSize),
                WorkManager.PartsForSeparate);
            
            var service = new ArrangePartsService(new List<Part>(parts), partsOverLength);
            MainWindowViewModel.DragAndDropData =
                new DragAndDropViewModel(service.GetArrangedRawMaterials(), service.GetOverSizeParts());
            
            
            var dragAndDropView = new DragAndDropView(mainWindow);
            if (!dockPanel.Children.Any())
            {
                dockPanel.Children.Add(dragAndDropView);
            }
            else
            {
                dockPanel.Children.RemoveAll(dockPanel.Children);
                dockPanel.Children.Add(dragAndDropView);
            }
        }
        else
        {
            MessageBox.Show(mainWindow, "선택된 파트가 없습니다.", "알림", MessageBox.MessageBoxButtons.YesNoCancel);
        }
    }
}