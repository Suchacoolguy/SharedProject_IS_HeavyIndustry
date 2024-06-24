using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

namespace SharedProject_IS_HeavyIndustry.Views.TabViews;

public partial class RawStandardTabView : TabView
{
    public RawStandardTabView()
    {
        InitializeComponent();
        DataContext = new RawStandardViewModel();
    }
}