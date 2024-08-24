using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.ViewModels;

namespace SharedProject_IS_HeavyIndustry.Converters
{
    public static class JsonConverter
    {
        private static string GetDatabasePath()
        {
            string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YourApplicationName");
            if (!Directory.Exists(appDataDirectory))
            {
                Directory.CreateDirectory(appDataDirectory);
            }
            return Path.Combine(appDataDirectory, "redyNest.db");
        }

        private static void MigrateDatabaseIfNeeded()
        {
            string sourceDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redyNest.db");
            string destDbPath = GetDatabasePath();

            if (File.Exists(sourceDbPath) && !File.Exists(destDbPath))
            {
                File.Copy(sourceDbPath, destDbPath);
            }
        }

        public static Dictionary<string, RawLengthSet>? ReadDictionaryFromJson()
        {
            try
            {
                MigrateDatabaseIfNeeded();
                string dbPath = GetDatabasePath();
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
                Console.WriteLine($"Error loading database: {ex.Message}");
                return null;
            }
        }

        public static void WriteDictionaryToJson(Dictionary<string, RawLengthSet> dictionary)
        {
            try
            {
                MigrateDatabaseIfNeeded();
                string dbPath = GetDatabasePath();
                string connectionDB = $"Data Source={dbPath}";

                using (SqliteConnection connection = new SqliteConnection(connectionDB))
                {
                    connection.Open();

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
                    
                    DeleteAllRawLengthSet();

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
                            selectCommand.Parameters.AddWithValue("@description", description);

                            using (var reader = selectCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
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
                                else
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

                    MessageService.Send("Successfully saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
            }
            SettingsViewModel.Refresh();
        }

        public static bool DeleteItemByDescription(string description)
        {
            try
            {
                string dbPath = GetDatabasePath();
                string connectionDB = $"Data Source={dbPath};";

                using (SqliteConnection connection = new SqliteConnection(connectionDB))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM RawLengthSet WHERE Description = @description";
                        command.Parameters.AddWithValue("@description", description);

                        int result = command.ExecuteNonQuery();

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
                MigrateDatabaseIfNeeded();
                string dbPath = GetDatabasePath();
                string connectionDB = $"Data Source={dbPath};";

                using (SqliteConnection connection = new SqliteConnection(connectionDB))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Key, Value FROM HyungGangSet";

                        using (var reader = command.ExecuteReader())
                        {
                            var dictionary = new Dictionary<string, string>();

                            while (reader.Read())
                            {
                                string key = reader["Key"].ToString();
                                string value = reader["Value"].ToString();

                                dictionary.Add(key, value);
                            }

                            return dictionary;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading database: {ex.Message}");
                return null;
            }
        }

        public static void WriteDictionaryToJson(Dictionary<string, string> dictionary)
        {
            try
            {
                MigrateDatabaseIfNeeded();
                string dbPath = GetDatabasePath();
                string connectionDB = $"Data Source={dbPath}";

                using (SqliteConnection connection = new SqliteConnection(connectionDB))
                {
                    connection.Open();

                    using (var createTableCommand = connection.CreateCommand())
                    {
                        createTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS HyungGangSet (
                            Key TEXT PRIMARY KEY NOT NULL,
                            Value TEXT
                        )";

                        createTableCommand.ExecuteNonQuery();
                    }

                    foreach (var kvp in dictionary)
                    {
                        string key = kvp.Key;
                        string value = kvp.Value;

                        using (var selectCommand = connection.CreateCommand())
                        {
                            selectCommand.CommandText = @"
                            SELECT Key, Value 
                            FROM HyungGangSet 
                            WHERE Key = @key";
                            selectCommand.Parameters.AddWithValue("@key", key);

                            using (var reader = selectCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string currentKey = reader["Key"].ToString();
                                    string currentValue = reader["Value"].ToString();

                                    bool isDifferent = currentKey != key || currentValue != value;

                                    if (isDifferent)
                                    {
                                        using (var updateCommand = connection.CreateCommand())
                                        {
                                            updateCommand.CommandText = @"
                                            UPDATE HyungGangSet
                                            SET Value = @value
                                            WHERE Key = @key";

                                            updateCommand.Parameters.AddWithValue("@value", value);
                                            updateCommand.Parameters.AddWithValue("@key", key);

                                            updateCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    using (var insertCommand = connection.CreateCommand())
                                    {
                                        insertCommand.CommandText = @"
                                        INSERT INTO HyungGangSet (Key, Value)
                                        VALUES (@key, @value)";

                                        insertCommand.Parameters.AddWithValue("@key", key);
                                        insertCommand.Parameters.AddWithValue("@value", value);

                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    MessageService.Send("Successfully saved.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
            }
            SettingsViewModel.Refresh();
        }

        private static string GetFilePath(string filename)
        {
            var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YourApplicationName", "Assets");
            if (!Directory.Exists(appDataDirectory))
            {
                Directory.CreateDirectory(appDataDirectory);
            }
            return Path.Combine(appDataDirectory, filename + ".json");
        }

        public static Dictionary<string, List<int>> LengthSetFromJson()
        {
            var origin = ReadDictionaryFromJson();
            if (origin == null || origin.Count == 0)
                throw new ArgumentNullException(nameof(origin), "The data does not exist.");
            var result = new Dictionary<string, List<int>>();
            foreach (var kvp in origin)
            {
                result[kvp.Key] = kvp.Value.LengthsAsIntegerList();
            }

            return result;
        }
        
        private static void DeleteAllRawLengthSet()
        {
            try
            {
                string dbPath = GetDatabasePath();
                string connectionDB = $"Data Source={dbPath};";

                using (SqliteConnection connection = new SqliteConnection(connectionDB))
                {
                    connection.Open();
                    using (connection)
                    {
                        connection.Open();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM RawLengthSet";
                            int result = command.ExecuteNonQuery();

                            Console.WriteLine($"Deleted {result} rows from RawLengthSet.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while trying to delete all items: {ex.Message}");
            }
        }
    }
}
