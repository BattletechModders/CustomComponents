using System;
using System.Collections.Generic;
using BattleTech.UI;

namespace CustomComponents
{
    public class InventorySorterNotListComparer : IComparer<InventoryItemElement_NotListView>
    {
        private readonly InventorySorterComparer comparer = new InventorySorterComparer();
        private readonly Comparison<InventoryItemElement_NotListView> wrapped;
        public InventorySorterNotListComparer(Comparison<InventoryItemElement_NotListView> comparison)
        {
            wrapped = comparison;
        }

        public int Compare(InventoryItemElement_NotListView a, InventoryItemElement_NotListView b)
        {
            var val = comparer.Compare(a?.ComponentRef?.Def, b?.ComponentRef?.Def);
            return val != 0 ? val : wrapped(a, b);
        }
    }
}