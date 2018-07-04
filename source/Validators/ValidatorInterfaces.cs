using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
  
    /// <summary>
    /// component check if it can be dropped to this location
    /// </summary>
    public interface IValidateDrop
    {
        /// <summary>
        /// validation drop check
        /// </summary>
        /// <param name="widget">location, where check</param>
        /// <param name="element">element being dragged</param>
        /// <returns></returns>
        IValidateDropResult ValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result);
    }

    /// <summary>
    /// component need to validate mech state
    /// </summary>
    public interface IMechValidate
    {
        /// <summary>
        /// validate mech
        /// </summary>
        /// <param name="errors">list of errors</param>
        /// <param name="validationLevel">level of validation</param>
        /// <param name="mechDef">mech to check</param>
        void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef);

        bool ValidateMechCanBeFielded(MechDef mechDef);
    }


    public delegate IValidateDropResult ValidateDropDelegate(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result);

    /// <summary>
    /// delegate for mech valudation
    /// </summary>
    /// <param name="errors">list of errors</param>
    /// <param name="validationLevel">level of validation</param>
    /// <param name="mechDef">mech to validate</param>
    public delegate void ValidateMechDelegate(Dictionary<MechValidationType, List<string>> errors,
        MechValidationLevel validationLevel, MechDef mechDef);


    public delegate bool ValidateMechCanBeFieldedDelegate(MechDef mechDef);
}