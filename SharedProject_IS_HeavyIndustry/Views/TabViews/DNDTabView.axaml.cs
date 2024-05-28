﻿using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;
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
        var panel = this.FindControl<Panel>("WorkSpace");
        
        List<Part> list = null;
        if (selectedType != null && selectedSize != null)
            list =  WorkManager.FindPartsByDescription(new Description(selectedType, selectedSize));
        foreach (var part in list)
        {
            Console.WriteLine(part);
        }
    }
}