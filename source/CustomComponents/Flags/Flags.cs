using System.Collections.Generic;
using CustomComponents.InventoryUnlimited;

namespace CustomComponents;

[CustomComponent("Flags")]
public class Flags : SimpleCustomComponent, IListComponent<string>
{
    private HashSet<string> flags;

    public override string ToString()
    {
        return $"Flags: [{string.Join(" ", flags)}]";
    }

    public void LoadList(IEnumerable<string> items)
    {
        flags = items.ToHashSet();
        CCFlags = new(this);

        if (CCFlags.InvUnlimited)
        {
            UnlimitedFeature.Register(Def);
        }
        Def.ccFlags = this;
    }

    // compatibility with CC <v2.0
    public bool IsSet(string value)
    {
        return flags.Contains(value);
    }

    internal CCFlags CCFlags;
}