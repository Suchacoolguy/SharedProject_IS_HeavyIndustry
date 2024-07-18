using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace SharedProject_IS_HeavyIndustry.Services;

public class DatabaseService
{
    const string connectionMemory = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";
    private static string connectionDB = "Data Source=/Users/suchacoolguy/Programming/db_test.db";
    private static string querySQL = "";

    private const string fileLocation =
        "/Users/suchacoolguy/RiderProjects/SharedProject_IS_HeavyIndustry/SharedProject_IS_HeavyIndustry/Assets/db_test.db";

    public DatabaseService()
    {
        // Get the directory where the executable is located
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        // Construct the path to the database file
        string dbPath = Path.Combine(exeDirectory, "db_test.db");

        // Use the database path in the connection string
        connectionDB = $"Data Source={dbPath}";
        
        Console.WriteLine("DB Path:" + dbPath);
    }

    static string createSelectQuery(string connectionString,string tableName)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            
        }
        return $"SELECT * FROM {tableName}";
    }

    static bool createRawLengthSetTable(string connectionString)
    {
        try
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
            
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = """
                                            CREATE TABLE RawLengthSet (
                                                desc TEXT,
                                                weight REAL,
                                                length TEXT
                                            )
                                            """;
                createCommand.ExecuteNonQuery();
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    static string updateSelectQuery(string query)
    {
        if (querySQL == "")
        {
            return "";
        }
        else
        {
            // 이미 조건문이 있으면 AND만 추가하면 됨.
            if (querySQL.Contains("where"))
            {
                
            }
            else    // 조건문이 없으면 WHERE도 함께 추가해야함.
            {
                
            }
        }

        return "";
    }
}