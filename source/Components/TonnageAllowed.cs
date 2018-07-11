using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    [CustomComponent("TonnageAllowed")]
    public class TonnageAllowed : SimpleCustomComponent, IMechLabFilter, IMechValidate, IPreValidateDrop
    {
        public int Tonnage { get; set; }

        public bool CheckFilter(MechLabPanel panel)
        {
            var tonnage = panel.activeMechDef.Chassis.Tonnage;
            return Tonnage == tonnage;
        }

        public string PreValidateDrop(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {
            Control.Logger.LogDebug("-- TonnageAllowed");
            var tonnage = location.mechLab.activeMechDef.Chassis.Tonnage;
            if (tonnage != Tonnage)
            {
                return $"{Def.Description.Name} designed for {Tonnage}t 'Mech";
            }

            return string.Empty;
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
