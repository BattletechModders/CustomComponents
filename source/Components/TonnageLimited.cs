using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component limited to mech tonnage
    /// </summary>
    [CustomComponent("TonnageLimit")]
    public class TonnageLimited : SimpleCustomComponent, IMechLabFilter, IMechValidate, IPreValidateDrop
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
            return tonnage >= Min && tonnage <= Max;
        }


        public string PreValidateDrop(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {
            Control.Logger.LogDebug("-- TonnageLimit");
            var tonnage = location.mechLab.activeMechDef.Chassis.Tonnage;


            if (tonnage < Min ||
                tonnage > Max)
            {
                if (Min == Max)
                    return $"{Def.Description.Name} designed for {Min}t 'Mech";
                else
                    return $"{Def.Description.Name} designed for {Min}t-{Max}t 'Mech";
            }

            return string.Empty;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if(mechDef.Chassis.Tonnage < Min && mechDef.Chassis.Tonnage > Max)

            if (Min == Max)
                errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(
                    $"{Def.Description.Name.ToUpper()} designed for {Min}t 'Mech"));
            else
                errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(
                    $"{Def.Description.Name.ToUpper()} designed for {Min}t-{Max}t 'Mech"));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            return mechDef.Chassis.Tonnage >= Min && mechDef.Chassis.Tonnage <= Max;
        }
    }
}
