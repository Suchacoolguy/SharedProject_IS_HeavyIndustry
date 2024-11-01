using System;
using System.Collections.Generic;
using System.Linq;
using SharedProject_IS_HeavyIndustry.Converters;

namespace SharedProject_IS_HeavyIndustry.ViewModels
{
    public static class SettingsViewModel
    {
        // 절단 손실분 (mm 단위)
        public static int CuttingLoss = 3;
        public static Dictionary<string, string> HyungGangSet { get; set; }
        public static Dictionary<string, List<int>> LengthOptionSet { get; set; }
        public static List<string> MissingKeys { get; private set; } = [];

        static SettingsViewModel()
        {
            InitializeLengthOptionSet();
            InitializeHyungGangSet();
        }

        private static void InitializeHyungGangSet()
        {
            try
            {
                HyungGangSet = JsonConverter.ReadHyungGangSetFromJson()!;
            }
            catch (Exception ex)
            {
                HyungGangSet = new Dictionary<string, string>();
                Console.WriteLine($"An error occurred while initializing LengthOptionSet: {ex.Message}");
            }
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
            InitializeHyungGangSet();
            InitializeLengthOptionSet();
            MissingKeys.Clear(); // Refresh할 때 누락된 키 리스트 초기화
        }

        public static int GetMaxLen(string desc)
        {
            if (LengthOptionSet.TryGetValue(desc, out var lengths))
                return lengths.Max();
            else
                Console.WriteLine($"Key not found: 규격목록에 {desc} 정보 없음");
            MissingKeys.Add(desc);
            return 0; // 혹은 다른 적절한 기본값을 반환
        }

        public static List<string> GetHyungGangList()
        {
            if (HyungGangSet == null)
                return [];
            return HyungGangSet.Count > 0 ? [..HyungGangSet.Keys] : [];
        }
    }
}
