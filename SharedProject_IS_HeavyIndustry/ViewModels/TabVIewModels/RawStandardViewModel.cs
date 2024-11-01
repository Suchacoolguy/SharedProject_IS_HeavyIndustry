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
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels
{
    public class RawStandardViewModel : INotifyPropertyChanged
    {
        public string Title { get; } = "규격 설정";
        public string SubTitle { get; } = "규격별 중량 및 원자재 길이를 정의할 수 있습니다\n규격 정보를 설정 할 수 있습니다";

        private Dictionary<string, RawLengthSet> LengthSetDictionary { get; set; }
        public static ObservableCollection<RawLengthSet> LengthSetList { get; set; } = null!;
        public static ObservableCollection<RawLengthSet> LengthSetListForUI { get; set; } = null!;
        
        private static HashSet<string> _missingStandardBuffer = new HashSet<string>();
        public static ObservableCollection<string> MissingStandardBuffer { get; } = new ObservableCollection<string>();

        public ICommand SaveCommand { get; }
        public ICommand PasteCommand { get; }

        public RawStandardViewModel()
        {
            // 여기 디비
            LengthSetDictionary = JsonConverter.ReadDictionaryFromJson() ?? new Dictionary<string, RawLengthSet>();
            LengthSetList = new ObservableCollection<RawLengthSet>(LengthSetDictionary.Values);
            LengthSetListForUI = Clone(LengthSetList);

            SaveCommand = new RelayCommand(Save);
            PasteCommand = new RelayCommand(Paste);

            LengthSetListForUI.CollectionChanged += LengthSetList_CollectionChanged!;
        }

        public static void AddMissingData()
        {
            if (MissingStandardBuffer.Count <= 0) return;
            foreach (var value in MissingStandardBuffer)
            {
                var newData = new RawLengthSet(value, 0, "");
                LengthSetList.Insert(0, newData);
                LengthSetListForUI.Insert(0, newData);
            }

            /*MissingStandardBuffer.Clear();
            _missingStandardBuffer.Clear();*/
            //MessageService.Send("규격목록에 정의 되지 않은 형강이 존재합니다.");
        }
        
        public static void AddToMissingStandardBuffer(string item)
        {
            if (_missingStandardBuffer.Add(item))
                MissingStandardBuffer.Add(item);
        }

        private void LengthSetList_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplyChanges();
        }

        private bool ApplyChanges()
        {
            LengthSetDictionary.Clear();
            var msg = "";
            var cnt = 0;
            foreach (var item in LengthSetList.Where(item => !string.IsNullOrWhiteSpace(item.Description)))
                if (!LengthSetDictionary.TryAdd(item.Description, item))
                {
                    msg += item.Description + ",\n";
                    cnt++;
                }

            if (cnt <= 0) return true;
            MessageService.Send("총 " + cnt + " 개의 중복된 규격이 존재합니다.\n" + msg);
            return false;
        }

        private void Save()
        {
            if (!CheckAllFieldFill())
            {
                MessageService.Send("길이가 설정되지 않은 규격이 있습니다.");
                return;
            }
            // 여기 디비
            if (ApplyChanges())
            {
                JsonConverter.WriteDictionaryToJson(LengthSetDictionary);
                SettingsViewModel.Refresh();
                
                //규격목록 수정후 저장할 때 테이블 뷰에 값이 있으면 반영(제외, 분리필요 체크)
                foreach (var part in BOMDataViewModel.AllParts)
                {
                    if (_missingStandardBuffer.Contains(part.Desc.ToString()))
                    {
                        part.IsExcluded = false;
                        if (SettingsViewModel.LengthOptionSet.ContainsKey(part.Desc.ToString()))
                        {
                            if (SettingsViewModel.LengthOptionSet[part.Desc.ToString()].Max() <= part.Length)
                                part.IsOverLenth = true;
                        }
                    }
                }
                MissingStandardBuffer.Clear();
                _missingStandardBuffer.Clear();
            }
            
        }

        private bool CheckAllFieldFill()
        {
            foreach (var value in LengthSetList)
                if (value.Lengths.Length == 0)
                    return false;
            return true;
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

            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.MainWindow;
            if (window == null) return;

            var result = await dialog.ShowAsync(window);

            if (result != null && result.Length > 0)
            {
                var filePath = result[0];
                var newLengthSetDictionary = ExcelDataReader.RawLengthSettingsFromExcel(filePath);

                if (newLengthSetDictionary != null)
                {
                    //JsonConverter.WriteDictionaryToJson(newLengthSetDictionary);
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
        
        private ObservableCollection<RawLengthSet> Clone(ObservableCollection<RawLengthSet> list)
        {
            ObservableCollection<RawLengthSet> result = [];
            foreach (var value in list)
                result.Add(value);
            return result;
        }
        
    }


    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
