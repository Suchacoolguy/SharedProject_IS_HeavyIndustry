﻿using System;
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

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels
{
    public class HyungGangViewModel : INotifyPropertyChanged
    {
        public string Title { get; } = "형강 설정";
        public string SubTitle { get; } = "정보 필요";
        private Dictionary<string, RawLengthSet> LengthSetDictionary { get; set; }
        public ObservableCollection<RawLengthSet> LengthSetList { get; set; }

        public ICommand SaveCommand { get; } 
        public ICommand PasteCommand { get; }

        public HyungGangViewModel()
        {
            LengthSetDictionary = JsonConverter.ReadDictionaryFromJson() ?? new Dictionary<string, RawLengthSet>();
            LengthSetList = new ObservableCollection<RawLengthSet>(LengthSetDictionary.Values);

            SaveCommand = new RelayCommand(Save);
            PasteCommand = new RelayCommand(Paste);

            LengthSetList.CollectionChanged += LengthSetList_CollectionChanged!;
        }

        private void LengthSetList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LengthSetDictionary = LengthSetList.ToDictionary(item => item.Description);
        }
        
        private void Save()
        {
            JsonConverter.WriteDictionaryToJson(LengthSetDictionary);
        }

        [Obsolete("Obsolete")]
        private async void Paste()
        {
            var dialog = new OpenFileDialog
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
                var newLengthSetDictionary = ExcelDataReader.RawLengthSettingsFromExcel(filePath);

                if (newLengthSetDictionary != null)
                {
                    JsonConverter.WriteDictionaryToJson(newLengthSetDictionary);
                    LengthSetDictionary = newLengthSetDictionary;
                    LengthSetList = new ObservableCollection<RawLengthSet>(LengthSetDictionary.Values);

                    // Raise PropertyChanged event to notify UI about the change
                    OnPropertyChanged(nameof(LengthSetList));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
