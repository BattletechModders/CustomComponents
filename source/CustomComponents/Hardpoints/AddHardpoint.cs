﻿using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using CustomComponents.ExtendedDetails;

namespace CustomComponents;

[CustomComponent("AddHardpoint")]
public class AddHardpoint : SimpleCustomComponent, IValueComponent<string>, IPreValidateDrop, IAdjustDescription, IValid
{
    public bool Valid => WeaponCategory != null && !WeaponCategory.Is_NotSet;

    public WeaponCategoryValue WeaponCategory { get; private set; }
    public HardpointInfo HPinfo { get; private set; }

    public void LoadValue(string value)
    {
        WeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(value);

        if (WeaponCategory == null)
        {
            WeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();
        }

        if (!WeaponCategory.Is_NotSet)
        {
            HPinfo = HardpointController.Instance[WeaponCategory];
        }
    }

    public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
    {
        if (!Valid)
        {
            return string.Empty;
        }

        var hardpoints = MechLabHelper.CurrentMechLab.ActiveMech.GetAllHardpoints(location);

        var n = 0;
        foreach (var  hardpoint in hardpoints)
        {
            if (hardpoint.hpInfo.Visible)
            {
                n += 1;
                if (hardpoint.hpInfo.WeaponCategory.ID == WeaponCategory.ID)
                {
                    return string.Empty;
                }
            }
        }

        if (n >= 4)
        {
            return Control.Settings.Message.Hardpoints_TooManyHardpoints;
        }

        return string.Empty;
    }

    public void AdjustDescription()
    {
        if (!Valid)
        {
            return;
        }

        var hpinfo = HardpointController.Instance[WeaponCategory];
        if (hpinfo == null || !hpinfo.Visible)
        {
            return;
        }

        ExtendedDetails.ExtendedDetails.GetOrCreate(Def).AddIfMissing(
            new ExtendedDetail
            {
                Index = Control.Settings.HardpointAddIndex,
                Identifier = "AddHardpoints",
                Text = "\n\nAdd Hardpoint: <b><color=" +Control.Settings.HardpointDescriptionColor + ">"
                       + WeaponCategory.FriendlyName
                       + "</color></b>"
            }
        );
    }
}

[CustomComponent("ReplaceHardpoint")]
public class ReplaceHardpoint : SimpleCustomComponent, IOnLoaded, IPreValidateDrop, IAdjustDescription, IValid
{
    public string UseHardpoint;
    public string AddHardpoint;

    public bool Valid => AddWeaponCategory != null && !AddWeaponCategory.Is_NotSet
                                                   && UseWeaponCategory != null && !UseWeaponCategory.Is_NotSet;

    public WeaponCategoryValue AddWeaponCategory { get; private set; }
    public WeaponCategoryValue UseWeaponCategory { get; private set; }

    public void OnLoaded()
    {
        AddWeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(AddHardpoint);

        if (AddWeaponCategory == null)
        {
            AddWeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();
        }

        UseWeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(UseHardpoint);

        if (UseWeaponCategory == null)
        {
            UseWeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();
        }
    }

    public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
    {
        return string.Empty;
    }

    public void AdjustDescription()
    {
        if (!Valid)
        {
            return;
        }

        var ahpinfo = HardpointController.Instance[AddWeaponCategory];
        var rhpinfo = HardpointController.Instance[UseWeaponCategory];

        ExtendedDetails.ExtendedDetails.GetOrCreate(Def).AddIfMissing(
            new ExtendedDetail
            {
                Index = Control.Settings.HardpointAddIndex,
                Identifier = "AddHardpoints",
                Text = "\n\nReplace <b><color=" + Control.Settings.HardpointDescriptionColor + ">"
                       + UseWeaponCategory.FriendlyName
                       + "</color></b> Hardpoint with <b><color=" + Control.Settings.HardpointDescriptionColor + ">"
                       + AddWeaponCategory.FriendlyName
                       + "</color></b>"

            }
        );
    }
}