using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents;

public static class InvItemExtensions
{
    public static IEnumerable<InvItem> ToInvItems(this IEnumerable<MechComponentRef> inventory)
    {
        return inventory.Select(i => new InvItem(i, i.MountedLocation));
    }
}


public class InvItem
{
    public MechComponentRef Item { get; set; }
    public ChassisLocations Location { get; set; }

    public InvItem(MechComponentRef item, ChassisLocations location)
    {
        Item = item;
        Location = location;
    }
}