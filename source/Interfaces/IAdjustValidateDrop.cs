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
        void ValidateDropOnAdd(MechLabItemSlotElement item, ChassisLocations location,Queue<IChange> changes);
        void ValidateDropOnRemove(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes);
    }

    //public delegate IEnumerable<IChange> ValidateAdjustDelegate(MechLabItemSlotElement item, ChassisLocations location, Queue<InvItem> changes);

}