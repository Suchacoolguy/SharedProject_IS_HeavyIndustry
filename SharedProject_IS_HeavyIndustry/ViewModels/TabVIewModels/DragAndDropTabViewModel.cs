using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Avalonia;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

public class DragAndDropTabViewModel : AvaloniaObject, INotifyPropertyChanged
{
    public DragAndDropTabViewModel()
    {
        filterSet = GetFilterSet();
        SetMaterialList();
    }

    public string Title { get; } = "배치 정보";
    public string SubTitle { get; } = "최적의 조합으로 파트를 배치합니다.";
    
    public readonly Dictionary<string, List<string>> filterSet; // 재질, 규격 중복 방지 딕셔너리

    private List<string> materialList; // 재질 콤보박스 아이템
    public List<string> MaterialList
    {
        get => materialList;
        set
        {
            materialList = value;
            materialList.Sort();
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> descriptionList = []; // 규격 콤보박스 아이템
    public ObservableCollection<string> DescriptionList
    {
        get => descriptionList;
        set
        {
            if (descriptionList != value)
            {
                // 알파벳 기준으로 정렬하고 숫자는 자연순으로 정렬
                descriptionList = new ObservableCollection<string>(
                    value.OrderBy(x => x, new AlphanumericComparer())
                );
                OnPropertyChanged();
            }
        }
    }

    /*public ObservableCollection<string> DescriptionList
    {
        get => descriptionList;
        set
        {
            descriptionList = value;
            var temp = descriptionList.ToList();
            temp.Sort();
            descriptionList = new ObservableCollection<string>(temp);
            OnPropertyChanged();
        }
    }*/

    private string selectedMaterial = ""; // 선택된 재질
    public string SelectedMaterial // 재질 선택시 규격 콤보박스 아이템 초기화
    {
        get => selectedMaterial;
        set
        {
            if (selectedMaterial == value) return;
            selectedMaterial = value;
            OnPropertyChanged();
            UpdateDescriptionList();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    //콤보박스 데이터 셋 만드는 코드 ------
    private Dictionary<string, List<string>> GetFilterSet()
    {
        var dictionary = new Dictionary<string, List<string>>();
        GetFilterSetFromParts(dictionary, BOMDataViewModel.AllParts);
        return dictionary;
    }

    private void GetFilterSetFromParts(Dictionary<string, List<string>> dictionary, ObservableCollection<Part> parts)
    {
        foreach (var part in parts)
        {
            if (part.IsExcluded) continue;
            if (dictionary.ContainsKey(part.Material)) continue;
            var descriptions = GetDescriptionList(part.Material, parts);
            dictionary.Add(part.Material, descriptions);
        }
    }
    
    private List<string> GetDescriptionList(string material,ObservableCollection<Part> parts) 
    {
        var duplicationCheckSet = new HashSet<string>();
        var descList = new List<string>();

        foreach (var part in parts) // 파트 목록을 순회하며 중복되지 않은 설명을 리스트에 추가
        {
            if (!part.IsExcluded && part.Material.Equals(material) && duplicationCheckSet.Add(part.Desc.ToString())) // HashSet에 현재 파트의 설명이 없는 경우 추가
                descList.Add(part.Desc.ToString());           // HashSet에 성공적으로 추가된 경우만 리스트에 추가
        }
        duplicationCheckSet.Clear();
        return descList;
    }
    //----------------------

    private void SetMaterialList()
    {
        MaterialList = new List<string>(filterSet.Keys);
    }

    private void UpdateDescriptionList()
    {
        if (filterSet.TryGetValue(SelectedMaterial, out var descriptions))
            DescriptionList = new ObservableCollection<string>(descriptions);
        else
            DescriptionList.Clear();
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    //문자열을 알파벳과 숫자로 비교하는 클래스
    public class AlphanumericComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // 정규식을 사용해 숫자와 알파벳 분리
            var regex = new Regex(@"(\D+)|(\d+)");
            var xMatches = regex.Matches(x);
            var yMatches = regex.Matches(y);

            int len = Math.Min(xMatches.Count, yMatches.Count);
            for (int i = 0; i < len; i++)
            {
                var xPart = xMatches[i].Value;
                var yPart = yMatches[i].Value;

                // 알파벳 비교
                if (char.IsLetter(xPart[0]) && char.IsLetter(yPart[0]))
                {
                    int result = string.Compare(xPart, yPart, StringComparison.Ordinal);
                    if (result != 0)
                        return result;
                }
                // 숫자 비교
                else if (char.IsDigit(xPart[0]) && char.IsDigit(yPart[0]))
                {
                    int xNumber = int.Parse(xPart);
                    int yNumber = int.Parse(yPart);

                    if (xNumber != yNumber)
                        return xNumber.CompareTo(yNumber);
                }
            }

            // 길이가 다를 경우 짧은 것이 먼저
            return xMatches.Count.CompareTo(yMatches.Count);
        }
    }
}
