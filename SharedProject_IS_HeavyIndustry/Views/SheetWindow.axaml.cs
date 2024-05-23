using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class SheetWindow : Window
{
    private List<string> SheetNames { get; }
    private readonly ComboBox? comboBox;
    public string? SelectedSheet { get; private set; }
    public SheetWindow(List<string> sheetNames, Window parent)
    {
        InitializeComponent();
        DataContext = this;
        SheetNames = sheetNames;
        comboBox = this.FindControl<ComboBox>("SheetList");
        if (comboBox != null) comboBox.ItemsSource = SheetNames;
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Cancel(object? sender, RoutedEventArgs e)
    {
        WorkManager.SheetName = null;
        Close(false);
    }

    private void Confirm(object? sender, RoutedEventArgs e)
    {
        WorkManager.SheetName =  (string)comboBox?.SelectedItem!;
        Close(true);
    }
}