using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> descriptionList = []; // 규격 콤보박스 아이템
    public ObservableCollection<string> DescriptionList
    {
        get => descriptionList;
        set
        {
            descriptionList = value;
            OnPropertyChanged();
        }
    }

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
}
