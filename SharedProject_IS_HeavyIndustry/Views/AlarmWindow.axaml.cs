using Avalonia.Controls;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class AlarmWindow : Window
{
    public AlarmWindow()
    {
        InitializeComponent();
        DataContext = new AlarmWindowViewModel();
    }
}