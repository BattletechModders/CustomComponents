using BattleTech.UI;
using System.Collections.Generic;

namespace CustomComponents
{
    /// <summary>
    /// component check if it can be dropped to this location
    /// </summary>
    public interface IAdjuctValidateDrop
    {
        /// <summary>
        /// validation drop check
        /// </summary>
        /// <param name="widget">location, where check</param>
        /// <param name="element">element being dragged</param>
        /// <returns></returns>
        List<IChange> ValidateDropOnAdd(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab);
        List<IChange> ValidateDropOnRemove(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab);
    }
}