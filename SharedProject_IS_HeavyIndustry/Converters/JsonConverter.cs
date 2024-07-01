using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Converters;

public static class JsonConverter
{
    private static string GetFilePath()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var filePath = Path.Combine(appDirectory, "Assets", "RawLengthSettingInfo.json");
        return filePath;
    }

    public static Dictionary<string, RawLengthSet>? ReadDictionaryFromJson()
    {
        try
        {
            var filePath = GetFilePath();
            if (!File.Exists(filePath))
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("알림", "규격정보가 없습니다", ButtonEnum.Ok);
                box.ShowAsync();
                return null;
            }


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
            var filePath = GetFilePath();
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 파일 저장 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - WriteDictionaryFromJson()");
        }
        SettingsViewModel.Refresh();
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

/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;

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
            // var filePath = Path.Combine(projectRoot, "Assets", "RawLengthSettingInfo.json");
            
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(appDirectory, "Assets", "RawLengthSettingInfo.json");
            Console.WriteLine("File Path: " + filePath);

            var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 파일 저장 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - WriteDictionaryFromJson()");
        }
        SettingsViewModel.Refresh();
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
}*/