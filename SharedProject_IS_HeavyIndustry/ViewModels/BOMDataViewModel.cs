using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class BOMDataViewModel
{
    public BOMDataViewModel(List<Part> parts)
    {
        ListParts = new ObservableCollection<Part>(parts);
    }
    
    public ObservableCollection<Part> ListParts { get; }
}