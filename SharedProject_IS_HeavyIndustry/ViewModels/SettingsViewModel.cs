using System;
using System.Collections.Generic;
using System.Linq;
using SharedProject_IS_HeavyIndustry.Converters;
using SharedProject_IS_HeavyIndustry.Services;

namespace SharedProject_IS_HeavyIndustry.ViewModels
{
    public static class SettingsViewModel
    {
        // public static double MaxLen = ArrangePartsService._lengthOptionsRawMaterial.Max();
        public static List<string> HyungGangList { get; set; } = [ "H", "I", "L", "C", "ㄷ", "TB" ];

        private static Dictionary<string, List<int>> LengthOptionSet { get; set; }
        public static List<string> MissingKeys { get; private set; } = [];

        static SettingsViewModel()
        {
            InitializeLengthOptionSet();
        }

        private static void InitializeLengthOptionSet()
        {
            try
            {
                LengthOptionSet = JsonConverter.LengthSetFromJson();
            }
            catch (Exception ex)
            {
                LengthOptionSet = new Dictionary<string, List<int>>();
                Console.WriteLine($"An error occurred while initializing LengthOptionSet: {ex.Message}");
            }
        }

        public static List<int> GetLengthOption(string desc)
        {
            if (LengthOptionSet.TryGetValue(desc, out var lengths))
            {
                return lengths;
            }
            else
            {
                Console.WriteLine($"Key not found: 규격목록에 {desc} 정보 없음");
                MissingKeys.Add(desc);
                return new List<int>();
            }
        }

        public static void Refresh()
        {
            InitializeLengthOptionSet();
            MissingKeys.Clear(); // Refresh할 때 누락된 키 리스트 초기화
        }

        public static double GetMaxLen(string desc)
        {
            if (LengthOptionSet.TryGetValue(desc, out var lengths))
            {
                return lengths.Max();
            }
            Console.WriteLine($"Key not found: 규격목록에 {desc} 정보 없음");
            MissingKeys.Add(desc);
            return 0.0; // 혹은 다른 적절한 기본값을 반환
        }
    }
}
