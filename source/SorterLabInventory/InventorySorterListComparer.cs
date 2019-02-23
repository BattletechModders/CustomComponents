using System;
using System.Collections.Generic;
using BattleTech.UI;

namespace CustomComponents
{
    public class InventorySorterListComparer : IComparer<InventoryDataObject_BASE>
    {
        private readonly InventorySorterComparer comparer = new InventorySorterComparer();
        private readonly Comparison<InventoryDataObject_BASE> wrapped;
        public InventorySorterListComparer(Comparison<InventoryDataObject_BASE> comparison)
        {
            wrapped = comparison;
        }

        public int Compare(InventoryDataObject_BASE a, InventoryDataObject_BASE b)
        {
            var val = comparer.Compare(a?.componentDef, b?.componentDef);
            return val != 0 ? val : wrapped(a, b);
        }
    }
}