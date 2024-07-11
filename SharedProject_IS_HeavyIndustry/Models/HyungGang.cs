namespace SharedProject_IS_HeavyIndustry.Models;

public class HyungGang
{
    public string Type { get; set; }
    public string Description { get; set; }

    public HyungGang(string type, string desc)
    {
        Type = type;
        Description = desc;
    }
}