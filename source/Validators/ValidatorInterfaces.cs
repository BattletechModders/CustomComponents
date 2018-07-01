using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component check if it can be added to this location
    /// </summary>
    public interface IValidateAdd
    {
        /// <summary>
        /// validation check
        /// </summary>
        /// <param name="widget">location, where check</param>
        /// <param name="current_result">result of previous checks</param>
        /// <param name="errorMessage">message, that show as warning if cannot add item</param>
        /// <param name="mechlab"></param>
        /// <returns></returns>
        bool ValidateAdd(MechLabLocationWidget widget, bool current_result, ref string errorMessage,
            MechLabPanel mechlab);
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

    /// <summary>
    /// delegate for "component can be added" validation
    /// </summary>
    /// <param name="component">component to validate</param>
    /// <param name="widget">location to add</param>
    /// <param name="current_result">resulr of previous сруслы</param>
    /// <param name="errorMessage">error message</param>
    /// <param name="mechlab"></param>
    /// <returns></returns>
    public delegate bool ValidateAddDelegate(MechComponentDef component, MechLabLocationWidget widget,
        bool current_result, ref string errorMessage, MechLabPanel mechlab);

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