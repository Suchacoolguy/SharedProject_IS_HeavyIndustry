﻿using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using Microsoft.Data.Sqlite;
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
            var filePath = GetFilePath("RawLengthSettingInfo");
            if (!File.Exists(filePath))
            {
                MessageService.Send("규격정보가 없습니다");
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
            var filePath = GetFilePath("RawLengthSettingInfo");
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            File.WriteAllText(filePath, json);
            MessageService.Send("성공적으로 저장되었습니다.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 파일 저장 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - WriteDictionaryFromJson()");
        }
        SettingsViewModel.Refresh();
        
        
        
    }
    
    public static Dictionary<string, string>? ReadHyungGangSetFromJson()
    {
        try
        {
            var filePath = GetFilePath("HyungGangSettingInfo");
            if (!File.Exists(filePath))
            {
                MessageService.Send("형강정보가 없습니다");
                return new Dictionary<string, string>();
            }

            var json = File.ReadAllText(filePath);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return dictionary;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 파일 로드 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - ReadHyungGangSetFromJson()");
            return new Dictionary<string, string>();
        }
    }
    public static void WriteDictionaryToJson(Dictionary<string, string> dictionary)
    {
        try
        {
            var filePath = GetFilePath("HyungGangSettingInfo");
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            File.WriteAllText(filePath, json);
            MessageService.Send("성공적으로 저장되었습니다.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON 파일 저장 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - WriteDictionaryFromJson()");
        }
        SettingsViewModel.Refresh();
    }
    
    private static string GetFilePath(string filename)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var filePath = Path.Combine(appDirectory, "Assets", filename + ".json");
        return filePath;
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