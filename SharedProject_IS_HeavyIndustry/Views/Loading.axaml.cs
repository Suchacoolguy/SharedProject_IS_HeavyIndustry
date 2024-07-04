using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class Loading : UserControl
{
    private Grid loadingGrid;

    public Loading()
    {
        InitializeComponent();
        loadingGrid = this.FindControl<Grid>("LoadingGrid")!;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void Start()
    {
        loadingGrid.IsVisible = true;
    }

    public void Stop()
    {
        loadingGrid.IsVisible = false;
    }
}