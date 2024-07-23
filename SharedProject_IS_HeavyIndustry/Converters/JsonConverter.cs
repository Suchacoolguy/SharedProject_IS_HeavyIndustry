using System;
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
            // Get the directory where the executable is located
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
    
            // Construct the path to the database file
            string dbPath = Path.Combine(exeDirectory, "db_test.db");
    
            // Use the database path in the connection string
            string connectionDB = $"Data Source={dbPath};";
    
            using (SqliteConnection connection = new SqliteConnection(connectionDB))
            {
                connection.Open();
        
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Description, Weight, Lengths FROM RawLengthSet";
                
                    using (var reader = command.ExecuteReader())
                    {
                        var dictionary = new Dictionary<string, RawLengthSet>();
                    
                        while (reader.Read())
                        {
                            string description = reader["Description"].ToString();
                            double weight = Convert.ToDouble(reader["Weight"]);
                            string lengths = reader["Lengths"].ToString();
    
                            dictionary.Add(description, new RawLengthSet(description, weight, lengths));
                        }
    
                        return dictionary;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"데이터베이스 로드 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : DBConverter.cs - ReadDictionaryFromDB()");
            return null;
        }
    }

    public static void WriteDictionaryToJson(Dictionary<string, RawLengthSet> dictionary)
    {
        /*try
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

        SettingsViewModel.Refresh();*/

    //Json 대신 DB 파일에 저장하는 코드. 추후에 적용할 예정

    try

    {
            // Get the directory where the executable is located
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
            // Construct the path to the database file
            string dbPath = Path.Combine(exeDirectory, "db_test.db");
        
            // Use the database path in the connection string
            string connectionDB = $"Data Source={dbPath}";
        
            Console.WriteLine("DB Path:" + dbPath);
        
            SqliteConnection connection = new SqliteConnection(connectionDB);
            connection.Open();
            
            
            // Create table if it doesn't exist
            using (var createTableCommand = connection.CreateCommand())
            {
                createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS RawLengthSet (
                    Description TEXT PRIMARY KEY NOT NULL,
                    Weight REAL,
                    Lengths TEXT
                )";
        
                createTableCommand.ExecuteNonQuery();
            }
        
            foreach (var kvp in dictionary)
            {
                string description = kvp.Key;
                RawLengthSet rawLengthSet = kvp.Value;
        
                using (var selectCommand = connection.CreateCommand())
                {
                    selectCommand.CommandText = @"
                    SELECT Description, Weight, Lengths 
                    FROM RawLengthSet 
                    WHERE Description = @description";
                    
                    // 바로 위에 있는 CommandText에서 @description라고 적어둔 곳에다 실제 description 값을 넣어줌
                    selectCommand.Parameters.AddWithValue("@description", description);
        
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        // 테이블에 값이 존재하믄
                        if (reader.Read())
                        {
                            // Record exists, check if values are different
                            string currentDescription = reader["Description"].ToString();
                            double currentWeight = Convert.ToDouble(reader["Weight"]);
                            string currentLengths = reader["Lengths"].ToString();
        
                            bool isDifferent = currentDescription != rawLengthSet.Description ||
                                               currentWeight != rawLengthSet.Weight ||
                                               currentLengths != rawLengthSet.Lengths;
        
                            if (isDifferent)
                            {
                                using (var updateCommand = connection.CreateCommand())
                                {
                                    updateCommand.CommandText = @"
                                    UPDATE RawLengthSet
                                    SET Weight = @weight, Lengths = @lengths
                                    WHERE Description = @description";
                                    
                                    updateCommand.Parameters.AddWithValue("@weight", rawLengthSet.Weight);
                                    updateCommand.Parameters.AddWithValue("@lengths", rawLengthSet.Lengths);
                                    updateCommand.Parameters.AddWithValue("@description", rawLengthSet.Description);
                                    
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            
                        }
                        else    // 테이블에 값이 없으믄
                        {
                            using (var insertCommand = connection.CreateCommand())
                            {
                                insertCommand.CommandText = @"
                                INSERT INTO RawLengthSet (Description, Weight, Lengths)
                                VALUES (@description, @weight, @lengths)";
                                
                                insertCommand.Parameters.AddWithValue("@description", rawLengthSet.Description);
                                insertCommand.Parameters.AddWithValue("@weight", rawLengthSet.Weight);
                                insertCommand.Parameters.AddWithValue("@lengths", rawLengthSet.Lengths);
        
                                insertCommand.ExecuteNonQuery();
                            }   
                        }
                    }
                }
            }
            
            MessageService.Send("성공적으로 저장되었습니다.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"데이터베이스 저장 중 오류 발생: {ex.Message}");
            Console.WriteLine("오류 발생 클래스 : JsonConverter.cs - WriteDictionaryFromJson()");
        }
        SettingsViewModel.Refresh();
    }
    
    public static bool DeleteItemByDescription(string description)
    {
        try
        {
            // Get the directory where the executable is located
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
    
            // Construct the path to the database file
            string dbPath = Path.Combine(exeDirectory, "db_test.db");
    
            // Use the database path in the connection string
            string connectionDB = $"Data Source={dbPath};";
    
            using (SqliteConnection connection = new SqliteConnection(connectionDB))
            {
                connection.Open();
        
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM RawLengthSet WHERE Description = @description";
                    command.Parameters.AddWithValue("@description", description);
                
                    int result = command.ExecuteNonQuery();
                
                    // Check if any row was actually deleted
                    if (result > 0)
                    {
                        Console.WriteLine("Item successfully deleted.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("No item found with the given description.");
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while trying to delete item: {ex.Message}");
            return false;
        }
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