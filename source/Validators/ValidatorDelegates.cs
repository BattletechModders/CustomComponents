using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{




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