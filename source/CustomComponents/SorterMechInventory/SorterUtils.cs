using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents;

internal static class SorterUtils
{
    internal static void SortMechDefInventory(MechComponentRef[] newInventory)
    {
        Array.Sort(newInventory, new InventoryComparer(newInventory));
    }

    internal static void SortWidgetInventory(List<MechLabItemSlotElement> inventory)
    {
        inventory.Sort(new InventoryComparer(inventory));
    }

    private class InventoryComparer : IComparer<MechComponentRef>, IComparer<MechLabItemSlotElement>
    {
        public InventoryComparer(List<MechLabItemSlotElement> elements)
            : this(elements.Select(x => x.ComponentRef).ToArray())
        {
        }

        public InventoryComparer(MechComponentRef[] originalOrder)
        {
            this.originalOrder = originalOrder;
        }

        public int Compare(MechLabItemSlotElement x, MechLabItemSlotElement y)
        {
            return Compare(x?.ComponentRef, y?.ComponentRef);
        }

        public int Compare(MechComponentRef x, MechComponentRef y)
        {
            var c = Order(x?.Def) - Order(y?.Def);
            if (c != 0)
            {
                return c;
            }
            // c# is not a stable sort, meaning equal elements are not guaranteed to keep order with each other
            // therefore we track the original sorting order and use the index as a secondary comparison
            return Index(x) - Index(y);
        }

        private static int Order(MechComponentDef def)
        {
            return def?.GetComponent<ISorter>()?.Order ?? Control.Settings.SorterMechInventoryDefault;
        }

        private readonly MechComponentRef[] originalOrder;
        private int Index(MechComponentRef r)
        {
            for (var i = 0; i < originalOrder.Length; i++)
            {
                var e = originalOrder[i];
                if (ReferenceEquals(r, e))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}