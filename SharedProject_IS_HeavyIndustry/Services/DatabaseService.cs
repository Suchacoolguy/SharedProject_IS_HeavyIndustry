using System.Linq;

namespace SharedProject_IS_HeavyIndustry.Services;

public class DatabaseService
{
    const string connectionMemory = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";
    const string connectionDB = "Data Source=/Users/suchacoolguy/Programming/db_test.db";
    private static string querySQL = "";

    private const string fileLocation =
        "/Users/suchacoolguy/RiderProjects/SharedProject_IS_HeavyIndustry/SharedProject_IS_HeavyIndustry/Assets/db_test.db";

    static string createSelectQuery(string tableName)
    {
        return $"SELECT * FROM {tableName}";
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