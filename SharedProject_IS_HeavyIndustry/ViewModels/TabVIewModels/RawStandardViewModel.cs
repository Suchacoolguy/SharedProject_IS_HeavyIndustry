using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Newtonsoft.Json;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Views.TabViews;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels
{
    public class RawStandardViewModel : AvaloniaObject
    {
        private Dictionary<string, RawLengthSet> LengthSetDictionary { get; set; }
        public ObservableCollection<RawLengthSet> LengthSetList { get; set; }

        public ICommand SaveCommand { get; }

        public RawStandardViewModel()
        {
            LengthSetDictionary = ReadDictionaryFromJson() ?? new Dictionary<string, RawLengthSet>();
            LengthSetList = new ObservableCollection<RawLengthSet>(LengthSetDictionary.Values);

            SaveCommand = new RelayCommand(Save);

            LengthSetList.CollectionChanged += LengthSetList_CollectionChanged!;
        }

        private void LengthSetList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LengthSetDictionary = LengthSetList.ToDictionary(item => item.Description);
        }
        
        private void Save()
        {
            WriteDictionaryToJson(LengthSetDictionary);
        }

        private static Dictionary<string, RawLengthSet>? ReadDictionaryFromJson()
        {
            try
            {
                var projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
                Debug.Assert(projectRoot != null, nameof(projectRoot) + " != null");
                var filePath = Path.Combine(projectRoot, "Assets", "RawLengthSettingInfo.json");

                var json = File.ReadAllText(filePath);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, RawLengthSet>>(json);
                return dictionary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON 파일 로드 중 오류 발생: {ex.Message}");
                Console.WriteLine("오류 발생 클래스 : RawStandardViewModel - ReadDictionaryFromJson()");
                return null;
            }
        }

        private void WriteDictionaryToJson(Dictionary<string, RawLengthSet> dictionary)
        {
            try
            {
                var projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
                Debug.Assert(projectRoot != null, nameof(projectRoot) + " != null");
                var filePath = Path.Combine(projectRoot, "Assets", "RawLengthSettingInfo.json");

                var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON 파일 저장 중 오류 발생: {ex.Message}");
                Console.WriteLine("오류 발생 클래스 : RawStandardViewModel - ReadDictionaryFromJson()");
            }
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
