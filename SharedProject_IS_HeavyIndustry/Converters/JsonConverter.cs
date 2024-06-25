using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using SharedProject_IS_HeavyIndustry.Models;

namespace SharedProject_IS_HeavyIndustry.Converters;

public static class JsonConverter
{
    public static Dictionary<string, RawLengthSet>? ReadDictionaryFromJson()
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
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - ReadDictionaryFromJson()");
            return null;
        }
    }
    
    public static void WriteDictionaryToJson(Dictionary<string, RawLengthSet> dictionary)
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
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - WriteDictionaryFromJson()");
        }
    }

    public static Dictionary<string, List<int>> LengthSetFromJson()
    {
        var origin = ReadDictionaryFromJson();
        if (origin == null || origin!.Count == 0 )
            throw new ArgumentNullException(nameof(origin), "The data does not exist.");
        var result = new Dictionary<string, List<int>>();
        foreach (var kvp in origin)
        {
            result[kvp.Key] = kvp.Value.LengthsAsIntegerList();
        }

        return result;
    }
}