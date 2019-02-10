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

        public string PreValidateDrop(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlabf)
        {
            Control.LogDebug(DType.ComponentInstall, "-- TonnageAllowed");
            var tonnage = location.mechLab.activeMechDef.Chassis.Tonnage;
            if (tonnage != Tonnage)
            {
                return $"{Def.Description.Name} designed for {Tonnage}t 'Mech";
            }

            return string.Empty;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (mechDef.Chassis.Tonnage != Tonnage)
            {
                errors[MechValidationType.InvalidInventorySlots].Add( new Localize.Text(
                    $"{Def.Description.Name} designed for {Tonnage}t mech"));
            }
        }


        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            return mechDef.Chassis.Tonnage == Tonnage;
        }


    }
}
