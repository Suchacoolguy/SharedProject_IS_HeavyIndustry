using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ClosedXML.Excel;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.ViewModels;
using SharedProject_IS_HeavyIndustry.Views;

namespace SharedProject_IS_HeavyIndustry;

public partial class App : Application
{
    [assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            // {
            //     DataContext = new MainWindowViewModel();
            // };
        }

        base.OnFrameworkInitializationCompleted();
    }
}