using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class AlarmWindowViewModel
{
    private static HashSet<string> _missingHyungGangBuffer = new HashSet<string>();
    private static HashSet<string> _missingStandardBuffer = new HashSet<string>();

    public static ObservableCollection<string> MissingHyungGangBuffer { get; } = new ObservableCollection<string>();
    public static ObservableCollection<string> MissingStandardBuffer { get; } = new ObservableCollection<string>();

    public static void AddToMissingHyungGangBuffer(string item)
    {
        if (_missingHyungGangBuffer.Add(item))
            MissingHyungGangBuffer.Add(item);
    }

    public static void AddToMissingStandardBuffer(string item)
    {
        if (_missingStandardBuffer.Add(item))
            MissingStandardBuffer.Add(item);
    }
}