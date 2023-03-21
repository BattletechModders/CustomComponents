using System;
using System.Collections.Generic;

namespace CustomComponents;

[CustomComponent("Flags")]
public class Flags : SimpleCustomComponent, IListComponent<string>
{
    [Obsolete("this should be internal, don't use directly from other mods")]
    public HashSet<string> flags;

    public override string ToString()
    {
        return $"Flags: [{string.Join(" ", flags)}]";
    }

    public void LoadList(IEnumerable<string> items)
    {
        flags = items.ToHashSet();
    }

    // compatibility with CC <v2.0
    public bool IsSet(string value)
    {
        return flags.Contains(value);
    }
}