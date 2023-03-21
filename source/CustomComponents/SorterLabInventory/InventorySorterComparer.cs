using System;
using System.Collections.Generic;
using BattleTech;

namespace CustomComponents;

public class InventorySorterComparer : IComparer<MechComponentDef>
{
    public static string SortKey(MechComponentDef def)
    {
        return def?.GetComponent<InventorySorter>()?.SortKey ?? Control.Settings.SorterLabInventoryDefault;
    }

    public int Compare(MechComponentDef x, MechComponentDef y)
    {
        var catA = SortKey(x);
        var catB = SortKey(y);
        var cmp = string.Compare(catA, catB, StringComparison.Ordinal);
        if (cmp != 0)
        {
            return cmp;
        }
        cmp = string.Compare(x?.Description.UIName, y?.Description.UIName, StringComparison.Ordinal);
        if (cmp != 0)
        {
            return cmp;
        }
        return string.Compare(x?.Description.Id, y?.Description.Id, StringComparison.Ordinal);
    }
}