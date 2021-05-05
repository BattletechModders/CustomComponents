using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    [CustomComponent("TonnageAllowed")]
    public class TonnageAllowed : SimpleCustomComponent, IMechLabFilter, IMechValidate, IPreValidateDrop, IValueComponent
    {
        public int Tonnage { get; set; }

        public bool CheckFilter(MechLabPanel panel)
        {
            var tonnage = panel.activeMechDef.Chassis.Tonnage;
            return Tonnage == tonnage;
        }


        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            Control.LogDebug(DType.ComponentInstall, "-- TonnageAllowed");
            var tonnage = MechLabHelper.CurrentMechLab.ActiveMech.Chassis.Tonnage;
            if (tonnage != Tonnage)
            {
                return (new Localize.Text(Control.Settings.Message.Tonnage_AddAllow, item.ComponentRef.Def.Description.UIName, Tonnage)).ToString();
            }

            return string.Empty;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (mechDef.Chassis.Tonnage != Tonnage)
            {
                errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(Control.Settings.Message.Tonnage_ValidateAllow, componentRef.Def.Description.UIName, Tonnage));
            }
        }


        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            return mechDef.Chassis.Tonnage == Tonnage;
        }


        public void LoadValue(object value)
        {
            Tonnage = value is Int64 i ? (int)i : 0;
        }
    }
}
