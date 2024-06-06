using System.Collections.Generic;
using System.Linq;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class SettingsViewModel
{
    public static double MaxLen = ArrangePartsService._lengthOptionsRawMaterial.Max();
    public static List<string> HyungGangList = new List<string>(){"H", "I", "L", "C", "ㄷ", "TB"};
}