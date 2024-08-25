using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SharedProject_IS_HeavyIndustry.Converters;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels
{
    public class HyungGangViewModel : INotifyPropertyChanged
    {
        public string Title { get; } = "형강 설정";
        public string SubTitle { get; } = "정보 필요";
        private Dictionary<string, string> HyungGangSet { get; set; }
        public ObservableCollection<HyungGang> HyungGangList { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand AutoInsertCommand { get; }

        public HyungGangViewModel()
        {
            HyungGangSet = JsonConverter.ReadHyungGangSetFromJson() ?? new Dictionary<string, string>();
            HyungGangList = new ObservableCollection<HyungGang>();
            foreach (var kvp in HyungGangSet)
                HyungGangList.Add(new HyungGang(kvp.Key, kvp.Value));

            SaveCommand = new RelayCommand(Save);
            PasteCommand = new RelayCommand(Paste);
            AutoInsertCommand = new RelayCommand(AutoInsertDefaults);

            HyungGangList.CollectionChanged += HyungGangList_CollectionChanged!;
        }

        private void HyungGangList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplyChanges();
        }

        private bool ApplyChanges()
        {
            HyungGangSet.Clear();
            var msg = "";
            var cnt = 0;
            foreach (var item in HyungGangList.Where(item => !string.IsNullOrWhiteSpace(item.Type)))
                if (!HyungGangSet.TryAdd(item.Type, item.Description))
                {
                    msg += item.Type + ",\n";
                    cnt++;
                }

            if (cnt <= 0) return true;
            MessageService.Send("총 " + cnt + " 개의 중복된 형강이 존재합니다.\n" + msg);
            return false;
        }

        private void Save()
        {
            if (ApplyChanges())
                JsonConverter.WriteDictionaryToJson(HyungGangSet);
        }
        
        private async void Paste()
        {
            /*var dialog = new OpenFileDialog
            {
                Title = "엑셀 파일 선택",
                Filters =
                [
                    new FileDialogFilter { Name = "Excel Files", Extensions = new List<string> { "xlsx", "xls" } }
                ]
            };

            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var result = await dialog.ShowAsync(window);

            if (result != null && result.Length > 0)
            {
                var filePath = result[0];
                var newHyungGangSet = ExcelDataReader.RawLengthSettingsFromExcel(filePath);

                if (newHyungGangSet != null)
                {
                    JsonConverter.WriteDictionaryToJson(newHyungGangSet);
                    HyungGangSet = newHyungGangSet;
                    HyungGangList = new ObservableCollection<RawLengthSet>(HyungGangSet.Values);

                    // Raise PropertyChanged event to notify UI about the change
                    OnPropertyChanged(nameof(HyungGangList));
                }
            }*/
        }

        private void AutoInsertDefaults()
        {
            if (!HyungGangList.Any())
            {
                var defaultData = new Dictionary<string, string>
                {
                    { "H", "H-BEAM" },
                    { "I", "I-BEAM" },
                    { "L", "ANGLE" },
                    { "C", "C-CHANNEL" },
                    { "ㄷ", "CHANNEL" },
                    { "RB", "환봉" },
                    { "D", "환봉" },
                    { "TB", "각관" },
                    { "PD", "PIPE" }
                };

                foreach (var kvp in defaultData)
                {
                    HyungGangList.Add(new HyungGang(kvp.Key, kvp.Value));
                }

                Save();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
