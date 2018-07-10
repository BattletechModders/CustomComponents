using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component check if it can be dropped to this location
    /// </summary>
    public interface IReplaceValidateDrop
    {
        /// <summary>
        /// validation drop check
        /// </summary>
        /// <param name="widget">location, where check</param>
        /// <param name="element">element being dragged</param>
        /// <returns></returns>
        IValidateDropResult ReplaceValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result);
    }
}