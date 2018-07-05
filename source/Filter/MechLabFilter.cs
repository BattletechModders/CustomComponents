using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public delegate bool FilterDelegate(MechLabHelper mechlab, MechComponentDef component);

    public static class MechLabFilter
    {
        private static List<FilterDelegate> filters = new List<FilterDelegate>();

        public static bool ApplyFilter(MechLabHelper mechlab, MechComponentDef component)
        {
            if (component is IHideFromInventory)
                return false;

            foreach (var filterDelegate in filters)
            {
                if (!filterDelegate(mechlab, component))
                    return false;
            }

            return true;
        }

        public static void AddFilter(FilterDelegate filter)
        {
            if(filter != null)
                filters.Add(filter);
        }
    }
}