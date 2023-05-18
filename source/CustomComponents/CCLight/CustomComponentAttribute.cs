using System;

namespace CustomComponents;

[AttributeUsage(AttributeTargets.Class)]
public class CustomComponentAttribute : Attribute
{
    public string Name { get; }

    public bool AllowArray { get; init; }

    [Obsolete("group is not supported anymore, AllowArray should be set using init syntax")]
    public CustomComponentAttribute(string name, bool allowarray = false, String group = "")
    {
        Name = name;
        AllowArray = allowarray;
    }

    public CustomComponentAttribute(string name)
    {
        Name = name;
    }
}