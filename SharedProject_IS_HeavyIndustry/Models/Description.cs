using System;

namespace SharedProject_IS_HeavyIndustry.Models;

public class Description
{
    private string type;
    private string size;

    public Description(string type, string size)
    {
        this.type = type;
        this.size = size;
    }
	
    public string Type
    {
        get { return type; }
        set { type = value; }
    }

    public string Size
    {
        get { return size; }
        set { size = value; }
    }
	
    public override string ToString()
    {
        return type + size;
    }

    public bool Equals(Description desc)
    {
        return type.Equals(desc.Type) && size.Equals(desc.Size);
    }
}