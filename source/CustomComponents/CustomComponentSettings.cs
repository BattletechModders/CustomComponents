using System;
using System.Collections.Generic;
using BattleTech.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomComponents;

public struct CCColor
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color ToColor()
    {
        return new(R / 255f, G / 255f, B / 255f, A / 255f);
    }
}

public struct TagColor
{
    public string Color { get; set; }
    public string Tag { get; set; }

    public Color ToColor()
    {
        if (ColorUtility.TryParseHtmlString(Color, out var color))
        {
            return color;
        }

        return UnityEngine.Color.magenta;
    }
}

[Serializable]
public class Tooltips
{
    public string JJCaption = "Jump Jet Hardpoints";
    public string JJTooltip = "A 'Mech can mount a limited number of Jump Jets, determined by its relative max speed. The number of Jump Jets a 'Mech can mount is represented by its Jump Jet 'hardpoint' total.";

    public string Alert_UnneededAmmo = "Unused ammo";
    public string Alert_Underweight = "Underweight";
    public string Alert_Overweight = "Overweight";
    public string Alert_Destroyed = "Location Destroyed";
    public string Alert_NoAmmo = "No ammo";
    public string Alert_MissingWeapon = "Missing weapon";
    public string Alert_Inventory = "Invalid equipment";
    public string Alert_Generic = "Error";
    public string Alert_Damaged = "Structure damaged";
}

[Serializable]
public class ErrorMessages
{
    public string WrongWeaponCount = "Cannot equip more then {0} weapons";

    public string Flags_InvaildComponent = "{0} has to be replaced";
    public string Flags_DestroyedComponent = "{0} is destroyed, replace it";
    public string Flags_DamagedComponent = "{0} is damaged, repair it";

    public string Tonnage_AddAllow = "{0} designed for {1}t 'Mech";
    public string Tonnage_ValidateAllow = "{0} designed for {1}t 'Mech";
    public string Tonnage_AddLimit = "{0} designed for {1}-{2}t 'Mech";
    public string Tonnage_ValidateLimit = "{0} designed for {1}-{2}t 'Mech";


    // 0 - Display Name, 1 - maximum, 2 - Mech Uiname, 3 - Mech Name
    // 4 - Location, 5 - item name, 6 - item uiname
    public string Category_MaximumReached = "Unit cannot install more {0} in {6}";
    public string Category_Mixed = "Mech can have only one type of {0}";

    // 0 - Display Name, 1 - Minimum, 2 - count, 3 - Mech Uiname, 4 - Mech Name
    // 6 - Location
    public string Category_ValidateMinimum = "MISSING {0}: This unit must mount at least {1} in {5}";
    public string Category_ValidateMaximum = "EXCESS {0}: This mech can't mount more then {1} in {5}";
    // 0 - Display Name
    public string Category_ValidateMixed = "Mech can have only one type of {0}";

    /// 0 - item Name, 1 - Location name, 2 - item.uiname
    public string Base_LocationDestroyed = "Cannot add {0} to {1}: The location is Destroyed.";
    /// 0 - item Name, 1 - Location name, 2 - item.uiname
    public string Base_AddWrongLocation = "Cannot add {0} to {1}: Component is not permitted in this location.";

    /// 0 - item Name, 1 - Location name, 2 - item.uiname
    public string Base_ValidateWrongLocation = "Unit have {0} installed in wrong location";

    /// <summary>
    /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - Hardpoint Name, 4 - Harpdpoint Friendly Name,
    /// 5 - Location
    /// </summary>
    public string Base_AddNoHardpoints = "Unit doesn't have {4} hardpoints in {5}";


    /// <summary>
    /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - Hardpoint Name, 4 - Harpdpoint Friendly Name,
    /// 5 - Location
    /// </summary>
    public string Base_AddNotEnoughHardpoints = $"Unit doesn't have enough {4} hardpoints in {5}";

    /// <summary>
    /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - Hardpoint Name, 4 - Harpdpoint Friendly Name,
    /// 5 - Location
    /// </summary>
    public string Base_ValidateNotEnoughHardpoints = "Unit doesn't have enough {1} hardpoints in {5}";

    public string Base_AddInventorySize = "Can't install {0} - not enough free space at {1}";

    /// <summary>
    /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - location
    /// </summary>
    public string Linked_Validate = "{1} installed wrong, reinstall it";

    public string Hardpoints_TooManyHardpoints = "Unit can have only 4 type of hardpoints per location";
    public string Hardpoints_NoReplace = "Unit dont have {0} hardpoint to replace in {1}";
}
public class CustomComponentSettings
{
    public bool DEBUG_DumpMechDefs = false;
    public string DEBUG_MechDefsDir = "D:/MechDefs";
    public bool DEBUG_ValidateMechDefs = false;
    public bool DEBUG_ShowOnlyErrors = false;
    public bool DEBUG_ShowAllUnitTypes = true;
    public bool DEBUG_EnableAllTags = false;
    public bool DEBUG_ShowConfig = false;
    public bool DEBUG_ShowLoadedCategory = true;
    public bool DEBUG_ShowLoadedDefaults = true;
    public bool DEBUG_ShowLoadedAlLocations = true;
    public bool DEBUG_ShowLoadedHardpoints = true;
    public bool DEBUG_DropValidationDisabled = false;

    public bool DEBUG_ShowMechUT = false;

    public List<TagColor> ColorTags = new();

    public bool OverrideSalvageGeneration = true;
    public bool NoLootCTDestroyed = false;
    public bool BaseECMValidation = true;

    public string SorterLabInventoryDefault = "pppppppp";
    public int SorterMechInventoryDefault = 100;

    public bool OverrideRecoveryChance = true;
    public bool SalvageUnrecoveredMech = true;
    public float LimbRecoveryPenalty = 0.05f;
    public float TorsoRecoveryPenalty = 0.1f;
    public float HeadRecoveryPenaly = 0;
    public float EjectRecoveryBonus = 0.25f;

    public bool OverrideMechPartCalculation = true;
    public int CenterTorsoDestroyedParts = 1;
    public float SalvageArmWeight = 0.75f;
    public float SalvageLegWeight = 0.75f;
    public float SalvageTorsoWeight = 1f;
    public float SalvageHeadWeight = 0.5f;


    public bool RunAutofixer = true;
    public bool FixDeletedComponents = true;
    public bool FixDefaults = true;
    public bool TagRestrictionDropValidateRequiredTags = false;
    public bool TagRestrictionDropValidateIncompatibleTags = true;
    public bool TagRestrictionUseMechTags = false;
    public bool CheckCriticalComponent = true;
    public bool UseDefaultFixedColor = false;
    public bool AddWeightToCategory = true;

    public UIColor DefaultFlagBackgroundColor = UIColor.UpgradeColor;
    public UIColor InvalidFlagBackgroundColor = UIColor.Red;

    public UIColor DefaultOverlayColor = UIColor.DarkGrayEighth;
    public CCColor PreinstalledOverlayCColor = new() { A = 12, R = 255, B = 180, G = 180 };
    public CCColor DefaultFlagOverlayCColor = new() { A = 12, R = 180, B = 180, G = 255 };

    [JsonIgnore] public Color PreinstalledOverlayColor;
    [JsonIgnore] public Color DefaultFlagOverlayColor;
    [JsonIgnore] public Dictionary<string, Color> ColorTagsDictionary;

    public string OmniTechFlag = "cc_omnitech";
    public bool OmniTechCostBySize = false;
    public int OmniTechInstallCost = 1;
    public bool DontUseFilter = false;
    public bool FixIcons = true;

    public bool CheckWeaponCount = false;
    public int MaxWeaponCount = 14;

    public string IgnoreAutofixUnitType = "IgnoreAutofix";
    public string IgnoreValidateUnitType = "IgnoreValidate";

    public bool CategoryDescriptionAddedByDefault = true;
    public int CategoryDescriptionIndex = 10;
    public bool HardpointDescriptionAddedByDefault = true;
    public int HardpointDescriptionIndex = 11;
    public int HardpointAddIndex = 12;
    public string HardpointDescriptionColor = "#008000";
    public string CategoryDescriptionColor = "#008000";

    public string[] IgnoreValidationTags = null;
    public TagUnitType[] UnitTypes = null;

    public bool AllowMechlabWrongHardpoints = false;

    public string CheckInvalidInventorySlotsDescription => "Make sure invalid mech configurations can't be saved in skirmish and can't be fielded in the campaign. Also allows a Mech with InvalidInventorySlots in SimGame to be saved anyway.";
    public bool CheckInvalidInventorySlots = true;

    public ErrorMessages Message = new();
    public Tooltips ToolTips = new();

    public int OmniCategoryID = 1000;

    public bool ColorHardpointsIcon = true;
    public UIColor HardpointIconDefaultColor = UIColor.White;
    public bool ColorHardpointsText = false;
    public UIColor HardpointTextDefaultColor = UIColor.White;
    public bool ColorHardpointsBack = false;
    public float HardpointBackAlpha = 0.8f;
    public Color HardpointBackDefaultColor = Color.black; // used if ColorHardpointsBack = false or for JJs back
    public UIColor HardpointJJTextAndIconColor = UIColor.White; // used if default colors not in use

    public Color GetHardpointBackDefaultColor()
    {
        var color = HardpointBackDefaultColor;
        color.a = HardpointBackAlpha;
        return color;
    }

    public void Complete()
    {
        PreinstalledOverlayColor = PreinstalledOverlayCColor.ToColor();
        DefaultFlagOverlayColor = DefaultFlagOverlayCColor.ToColor();

        ColorTagsDictionary = new();
        if (ColorTags != null)
        {
            foreach (var colorTag in ColorTags)
            {
                if (!ColorTagsDictionary.ContainsKey(colorTag.Tag))
                {
                    ColorTagsDictionary.Add(colorTag.Tag, colorTag.ToColor());
                }
            }
        }

        if(ToolTips == null)
        {
            ToolTips = new();
        }
    }
}