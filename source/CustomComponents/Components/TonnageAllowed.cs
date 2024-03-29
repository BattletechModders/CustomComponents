﻿using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Localize;

namespace CustomComponents;

[CustomComponent("TonnageAllowed")]
public class TonnageAllowed : SimpleCustomComponent, IMechLabFilter, IMechValidate, IPreValidateDrop, IValueComponent<int>
{
    public int Tonnage { get; set; }

    public bool CheckFilter(MechLabPanel panel)
    {
        var tonnage = panel.activeMechDef.Chassis.Tonnage;
        return Tonnage == tonnage;
    }


    public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
    {
        Log.ComponentInstall.Trace?.Log("-- TonnageAllowed");
        var tonnage = MechLabHelper.CurrentMechLab.ActiveMech.Chassis.Tonnage;
        if (tonnage != Tonnage)
        {
            return (new Text(Control.Settings.Message.Tonnage_AddAllow, item.ComponentRef.Def.Description.UIName, Tonnage)).ToString();
        }

        return string.Empty;
    }

    public void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
    {
        if (mechDef.Chassis.Tonnage != Tonnage)
        {
            errors[MechValidationType.InvalidInventorySlots].Add(new(Control.Settings.Message.Tonnage_ValidateAllow, componentRef.Def.Description.UIName, Tonnage));
        }
    }


    public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
    {
        return mechDef.Chassis.Tonnage == Tonnage;
    }


    public void LoadValue(int value)
    {
        Tonnage = value;
    }
}