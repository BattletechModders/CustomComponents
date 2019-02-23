using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public class SorterComparer : IComparer<MechComponentDef>
    {
        private static int Order(MechComponentDef def)
        {
            return def?.GetComponent<ISorter>()?.Order ?? 100;
        }

        public int Compare(MechComponentDef x, MechComponentDef y)
        {
            return Order(x) - Order(y);
        }
    }
}
