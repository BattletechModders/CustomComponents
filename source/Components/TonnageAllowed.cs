using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    [CustomComponent("TonnageAllowed")]
    public class TonnageAllowed : SimpleCustomComponent, IMechLabFilter, IPostValidateDrop, IMechValidate
    {
        public int Tonnage { get; set; }

        public bool CheckFilter(MechLabPanel panel)
        {
            var tonnage = panel.activeMechDef.Chassis.Tonnage;
            return Tonnage == tonnage;
        }

        public IValidateDropResult PostValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            var tonnage = location.mechLab.activeMechDef.Chassis.Tonnage;
            if (tonnage != Tonnage)
            {
                return new ValidateDropError($"{Def.Description.Name} designed for {Tonnage}t 'Mech");
            }

            return last_result;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<string>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            if(mechDef.Chassis.Tonnage != Tonnage)
            {
                errors[MechValidationType.InvalidInventorySlots].Add(
                    $"{Def.Description.Name} designed for {Tonnage}t mech");
            }
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            return mechDef.Chassis.Tonnage == Tonnage;
        }
    }
}
