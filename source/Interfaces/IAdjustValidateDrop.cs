using BattleTech.UI;
using System.Collections.Generic;

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
        IEnumerable<IChange> ValidateDropOnAdd(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab);
        IEnumerable<IChange> ValidateDropOnRemove(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab);
    }
}