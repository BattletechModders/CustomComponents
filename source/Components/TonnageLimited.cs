using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Localize;

namespace CustomComponents;

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
        if (panel == null)
        {
            Log.Main.Error?.Log("TonnageLimited.CheckFilter: MechLab is null");
            return true;
        }
        if (panel.activeMechDef == null)
        {
            Log.Main.Error?.Log("TonnageLimited.CheckFilter: MechDef is null");
            return true;
        }
        if (panel.activeMechDef.Chassis == null)
        {
            Log.Main.Error?.Log("TonnageLimited.CheckFilter: MechDef.Chassis is null");
            return true;
        }

        var tonnage = panel.activeMechDef.Chassis.Tonnage;
        return tonnage >= Min && tonnage <= Max;
    }



    public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
    {
        Log.ComponentInstall.Trace?.Log("-- TonnageLimit");
        var tonnage = MechLabHelper.CurrentMechLab.ActiveMech.Chassis.Tonnage;


        if (tonnage < Min ||
            tonnage > Max)
        {
            if (Min == Max)
                return (new Text(Control.Settings.Message.Tonnage_AddAllow, item.ComponentRef.Def.Description.UIName, Min)).ToString();
            else
                return (new Text(Control.Settings.Message.Tonnage_AddLimit, item.ComponentRef.Def.Description.UIName, Min, Max)).ToString();
        }

        return string.Empty;
    }

    public void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
    {
        if (mechDef.Chassis.Tonnage < Min && mechDef.Chassis.Tonnage > Max)

            if (Min == Max)
                errors[MechValidationType.InvalidInventorySlots].Add(new Text(Control.Settings.Message.Tonnage_ValidateAllow, componentRef.Def.Description.UIName, Min));
            else
                errors[MechValidationType.InvalidInventorySlots].Add(new Text(Control.Settings.Message.Tonnage_ValidateLimit, componentRef.Def.Description.UIName, Min, Max));
    }

    public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
    {
        return mechDef.Chassis.Tonnage >= Min && mechDef.Chassis.Tonnage <= Max;
    }
}