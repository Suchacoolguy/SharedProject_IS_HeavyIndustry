using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Spreadsheet;
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
            filter.ShowAt(button);
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
    }
}
