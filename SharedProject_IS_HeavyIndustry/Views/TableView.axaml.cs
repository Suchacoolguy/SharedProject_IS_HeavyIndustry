using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Spreadsheet;
using ReactiveUI;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Views
{
    public partial class TableView : UserControl
    {
        private Dictionary<string, FilteringService> filterSet = new();
        public TableView()
        {
            InitializeComponent();
        }

        [Obsolete("Obsolete")]
        private void Filter_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var columnHeader = button.Tag?.ToString();

            var filter = FilteringService.GetFilterMenu(columnHeader!);

            FlyoutBase.SetAttachedFlyout(button, filter);
            filter?.ShowAt(button);
        }

        public void SetExclude(bool value)
        {
            var table = this.FindControl<DataGrid>("Table");

            if (table == null) return;
            foreach (var item in table.ItemsSource.Cast<Part>())
                item.IsExcluded = value;
        }

        public void SetSeparate(bool value)
        {
            var table = this.FindControl<DataGrid>("Table");

            if (table == null) return;
            foreach (var item in table.ItemsSource.Cast<Part>())
                item.NeedSeparate = value;
        }
        
        private void Table_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Column is DataGridTextColumn textColumn &&
                textColumn.Tag?.ToString() == "LengthToBeSeparated")
            {
                var textBox = e.EditingElement as TextBox;
                if (textBox != null)
                {
                    if (textBox.Text != null)
                    {
                        string newValue = textBox.Text.Trim();
                        if (e.Row.DataContext is Part part)
                        {
                            Console.WriteLine("sucess");
                            part.lengthToBeSeparated = newValue;
                        
                            Console.WriteLine($"Before setting: Part {part.IsOverLenth} 분리길이: {part.lengthToBeSeparated}");
                        }
                    }
                }
            }
        }

        // public void abc()
        // {
        //     var dataGrid = this.FindControl<DataGrid>("Table");
        //     dataGrid.CellEditEnded
        //     
        //     foreach (var column in dataGrid.Columns)
        //     {
        //         if (column.Header.ToString() == "분리길이")
        //             Console.WriteLine(column);
        //     }
        // }
    }
}
