using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component limited to mech tonnage
    /// </summary>
    [CustomComponent("TonnageLimit")]
    public class TonnageLimited : SimpleCustomComponent, IMechLabFilter, IPostValidateDrop, IMechValidate
    {
        /// <summary>
        /// minimum allowed tonnage
        /// </summary>
        public int Min { get; set; }
        /// <summary>
        /// maximum allowed tonnage
        /// </summary>
        public int Max { get; set; }

        public bool CheckFilter(MechLabPanel panel)
        {
            var tonnage = panel.activeMechDef.Chassis.Tonnage;
            return Min >= tonnage && Max <= tonnage;
        }

        public IValidateDropResult PostValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            var tonnage = location.mechLab.activeMechDef.Chassis.Tonnage;


            if (tonnage < Min ||
                tonnage > Max)
            {
                if (Min == Max)
                    return new ValidateDropError($"{element.ComponentRef.Def.Description.Name} designed for {Min}t 'Mech");
                else
                    return new ValidateDropError($"{element.ComponentRef.Def.Description.Name} designed for {Min}t-{Max}t 'Mech");
            }

            return last_result;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<string>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            if(mechDef.Chassis.Tonnage < Min && mechDef.Chassis.Tonnage > Max)

            if (Min == Max)
                errors[MechValidationType.InvalidInventorySlots].Add(
                    $"{Def.Description.Name.ToUpper()} designed for {Min}t 'Mech");
            else
                errors[MechValidationType.InvalidInventorySlots].Add(
                    $"{Def.Description.Name.ToUpper()} designed for {Min}t-{Max}t 'Mech");
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            return mechDef.Chassis.Tonnage >= Min && mechDef.Chassis.Tonnage <= Max;
        }
    }
}
