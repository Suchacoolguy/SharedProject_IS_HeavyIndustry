using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.ViewModels.TabVIewModels;

public class DNDTabViewModel : AvaloniaObject
{
    public DNDTabViewModel()
    {
        SetTypeList();
    }
    
    public string Title { get; } = "배치 정보";
    public string SubTitle { get; } = "최적의 조합으로 파트를 배치합니다.";
    private readonly List<Description> descriptionList = BOMDataViewModel.GetDescriptionList();
    public ObservableCollection<string> TypeList { get; } = [];

    private string selectedType;
    public string SelectedType
    {
        get => selectedType;
        set
        {
            if (selectedType == value) return;
            selectedType = value;
            OnPropertyChanged();
            UpdateSizeList();
        }
    }
    
    private ObservableCollection<string> sizeList = [];
    public ObservableCollection<string> SizeList
    {
        get => sizeList;
        set
        {
            sizeList = value;
            OnPropertyChanged();
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    public static ObservableCollection<Part> FindPartsByDescription(Description desc, ObservableCollection<Part> parts) // DNDTabView에서 사용
    {
        var list = new ObservableCollection<Part>();
        
        foreach (var part in parts)
            if(part.Desc.Equals(desc))
                list.Add(part);
        return list;
    }

  

    private void SetTypeList()
    {
        var duplicationCheckSet = new HashSet<string>();

        foreach (var desc in descriptionList) // 파트 목록을 순회하며 중복되지 않은 설명을 리스트에 추가
        {
            if (duplicationCheckSet.Add(desc.Type)) // HashSet에 현재 파트의 설명이 없는 경우 추가
                TypeList.Add(desc.Type);                           // HashSet에 성공적으로 추가된 경우만 리스트에 추가
        }
    }
    
    private void UpdateSizeList()
    {
        SizeList.Clear();

        var duplicationCheckSet = new HashSet<string>();

        foreach (var desc in descriptionList) // 파트 목록을 순회하며 중복되지 않은 설명을 리스트에 추가
        {
            if (desc.Type.Equals(selectedType) && duplicationCheckSet.Add(desc.Size)) // HashSet에 현재 파트의 설명이 없는 경우 추가
                SizeList.Add(desc.Size);                           // HashSet에 성공적으로 추가된 경우만 리스트에 추가
        }
    }
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}