using System;

namespace CustomComponents;

[AttributeUsage(AttributeTargets.Property)]
internal class CustomFlagAttribute : Attribute
{
    public string FlagName { get; private set; }

    public CustomFlagAttribute(string name)
    {
        FlagName = name;
    }
}

[AttributeUsage(AttributeTargets.Property)]
internal class SubFlagsAttribute : Attribute
{
    public string[] Childs { get; private set; }

    public SubFlagsAttribute(params string[] childs)
    {
        Childs = childs;
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal class CustomSetterAttribute : Attribute
{
    public string Flag { get; private set;  }

    public CustomSetterAttribute(string flag)
    {
        Flag = flag;
    }
}