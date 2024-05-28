using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Views;

public partial class OverSizePartsView : UserControl
{
    public OverSizePartsView()
    {
        InitializeComponent();
    }

    private void Part_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var part = (sender as Control)?.DataContext as Part;
        
        if (part != null)
        {
            // the part object being dragged
            var data = new DataObject();
            data.Set("part", part);
            
            DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
        }
    }
}