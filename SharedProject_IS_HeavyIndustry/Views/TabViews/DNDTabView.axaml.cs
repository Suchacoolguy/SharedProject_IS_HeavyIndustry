using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class DNDTabView : TabView
{
    private StartWindow mainWindow;
    public DNDTabView()
    {
        InitializeComponent();
        DataContext = new DNDTabViewModel();
    }
    
    public DNDTabView(StartWindow mainWindow)
    {
        InitializeComponent();
        this.mainWindow = mainWindow;
        DataContext = new ExcelTabViewModel();
    }
}