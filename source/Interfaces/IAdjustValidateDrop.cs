using BattleTech.UI;
using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    /// <summary>
    /// component check if it can be dropped to this location
    /// </summary>
    public interface IAdjustValidateDrop
    {
        /// <summary>
        /// validation drop check
        /// </summary>
        /// <param name="widget">location, where check</param>
        /// <param name="element">element being dragged</param>
        /// <returns></returns>
        bool ValidateDropOnAdd(MechLabItemSlotElement item, ChassisLocations location,Queue<IChange> changes, List<InvItem> inventory);
        bool ValidateDropOnRemove(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<InvItem> inventory);
    }

    //public delegate IEnumerable<IChange> ValidateAdjustDelegate(MechLabItemSlotElement item, ChassisLocations location, Queue<InvItem> changes);

}