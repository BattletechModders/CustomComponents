using System;
using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
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
            return string.Compare(catA, catB, StringComparison.Ordinal);
        }
    }
}
