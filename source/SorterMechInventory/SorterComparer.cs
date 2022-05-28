using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    internal static class SorterComparer
    {
        private static int Order(MechComponentDef def)
        {
            return def?.GetComponent<ISorter>()?.Order ?? 100;
        }

        private static int CompareDef(MechComponentDef x, MechComponentDef y)
        {
            return Order(x) - Order(y);
        }

        internal static int CompareRef(MechComponentRef x, MechComponentRef y)
        {
            return CompareDef(x?.Def, y?.Def);
        }

        internal static int CompareElement(MechLabItemSlotElement x, MechLabItemSlotElement y)
        {
            return CompareRef(x?.ComponentRef, y?.ComponentRef);
        }
    }
}
