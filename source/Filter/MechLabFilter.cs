using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    /// <summary>
    /// Delegate for do additional filtering in mechlab inventory
    /// </summary>
    /// <param name="mechlab">active mechlab panel</param>
    /// <param name="component">component to check</param>
    /// <returns>show item in list</returns>
    public delegate bool FilterDelegate(MechLabHelper mechlab, MechComponentDef component);


    /// <summary>
    /// Controller to do additional filtering in mechlab inventory
    /// </summary>
    public static class MechLabFilter
    {
        private static List<FilterDelegate> filters = new List<FilterDelegate>();

        internal static bool ApplyFilter(MechLabHelper mechlab, MechComponentDef component)
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

        /// <summary>
        /// Add custom filter
        /// </summary>
        /// <param name="filter"></param>
        public static void AddFilter(FilterDelegate filter)
        {
            if(filter != null)
                filters.Add(filter);
        }
    }
}