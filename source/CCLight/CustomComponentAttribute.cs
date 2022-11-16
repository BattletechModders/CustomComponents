using System;

namespace CustomComponents;

[AttributeUsage(AttributeTargets.Class)]
public class CustomComponentAttribute : Attribute
{
    public string Name { get; set; }
    public string ArrayName { get; set; }

    public bool AllowArray { get; set; }
    public string Group { get; set; }

    public CustomComponentAttribute(string name, bool allowarray = false, String group = "")
    {
        Name = name;
        Group = group;
        AllowArray = allowarray;
    }

    public CustomComponentAttribute(string name) : this(name,false)
    { }
}