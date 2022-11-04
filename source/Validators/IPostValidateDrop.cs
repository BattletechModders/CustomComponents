using BattleTech.UI;
using System.Collections.Generic;

namespace CustomComponents
{
    /// <summary>
    /// component check if it can be dropped to this location
    /// </summary>
    public interface IPostValidateDrop
    {
        /// <summary>
        /// validation drop check
        /// </summary>
        /// <param name="widget">location, where check</param>
        /// <param name="element">element being dragged</param>
        /// <returns></returns>
        string PostValidateDrop(MechLabItemSlotElement drop_item, List<InvItem> new_inventory);
    }

    public delegate string PostValidateDropDelegate(MechLabItemSlotElement drop_item, List<InvItem> new_inventory);
}