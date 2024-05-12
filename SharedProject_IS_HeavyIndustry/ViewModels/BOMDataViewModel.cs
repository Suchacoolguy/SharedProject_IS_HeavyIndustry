using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class BOMDataViewModel
{
    public BOMDataViewModel(List<Part> parts)
    {
        ListParts = new ObservableCollection<Part>(parts);
        // Console.WriteLine(ListParts.GetType());
        // foreach (var part in ListParts)
        // {
        //     Console.WriteLine("Length:" + part.Length);
        // }
    }
    
    public ObservableCollection<Part> ListParts { get; }
}