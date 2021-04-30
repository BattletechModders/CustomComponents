using System;
using System.Collections.Generic;
using BattleTech.UI;
using HBS.Collections;
using HBS.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomComponents
{
    [SerializeField]
    public struct CCColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color ToColor()
        {
            return new Color(R / 255f, G / 255f, B / 255f, A / 255f);
        }
    }

    [SerializeField]
    public struct TagColor
    {
        public string Color { get; set; }
        public string Tag { get; set; }

        public Color ToColor()
        {
            if (ColorUtility.TryParseHtmlString(Color, out var color))
                return color;
            return UnityEngine.Color.magenta;
        }
    }

    [Flags]

    public enum DType
    {
        NONE = 0,
        SalvageProccess = 1,
        Hardpoints = 1 << 1,
        RESERVED0 = 1 << 2,
        ComponentInstall = 1 << 3,
        MechValidation = 1 << 4,
        CCLoading = 1 << 5,
        CCLoadingSummary = 1 << 6,
        InstallCost = 1 << 7,
        FixedCheck = 1 << 8,
        DefaultHandle = 1 << 9,
        ClearInventory = 1 << 10,
        CustomResource = 1 << 11,
        Flags = 1 << 12,
        AutoFix = 1 << 13,
        Filter = 1 << 14,
        Color = 1 << 15,
        Icons = 1 << 16,
        AutoFixFAKE = 1 << 17,
        AutoFixBase = 1 << 18,
        RESERVED1 = 1 << 19,
        AutofixValidate = 1 << 20,
        UnitType = 1 << 21,
        FULL = 0xffffff,
    }

    public class ErrorMessages
    {
        public string WrongWeaponCount = "Cannot equip more then {0} weapons";
        
        public string Flags_InvaildComponent = "{0} is placeholder, remove it";
        public string Flags_DestroyedComponent = "{0} is destroyed, replace it";
        public string Flags_DamagedComponent = "{0} is damaged, repair it";

        public string Tonnage_AddAllow = "{0} designed for {1}t 'Mech";
        public string Tonnage_ValidateAllow = "{0} designed for {1}t 'Mech";
        public string Tonnage_AddLimit = "{0} designed for {1}-{2}t 'Mech";
        public string Tonnage_ValidateLimit = "{0} designed for {1}-{2}t 'Mech";


        // 0 - Display Name, 1 - maximum, 2 - Mech Uiname, 3 - Mech Name
        // 4 - Location, 5 - item name, 6 - item uiname
        public string Category_MaximumReached = "Unit Cannot install more {0} in {6}";
        public string Category_Mixed = "Mech can have only one type of {0}";

        // 0 - Display Name, 1 - Minimum, 2 - count, 3 - Mech Uiname, 4 - Mech Name
        // 6 - Location
        public string Category_ValidateMinimum = "MISSING {0}: This unit must mount at least {1} in {5}";
        public string Category_ValidateMaximum = "EXCESS {0}: This mech can't mount more then {1} in {5}";
        // 0 - Display Name
        public string Category_ValidateMixed = "WRONG {0}: Mech can have only one type of {0}";

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
        public string Base_AddNoHardpoins = "{0} dont have {4} hardpoints in {5}";


        /// <summary>
        /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - Hardpoint Name, 4 - Harpdpoint Friendly Name,
        /// 5 - Location 
        /// </summary>
        public string Base_AddNotEnoughHardpoints = $"Unit dont have enough {4} hardpoints in {5}";

        /// <summary>
        /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - Hardpoint Name, 4 - Harpdpoint Friendly Name,
        /// 5 - Location 
        /// </summary>
        public string Base_ValidateNotEnoughHardpoints = "Unit dont have enough {1} hardpoints in {5}";

        public string Base_AddInventorySize = "Can't install {0} - not enough free space at {1}";

        /// <summary>
        /// 0 - mech.Uiname, 1 - item.Name, 2 - item.Uiname, 3 - location
        /// </summary>
        public string Linked_Validate = "{1} installed wrong, reinstall it";
    }
    public class CustomComponentSettings
    {
        public DType DebugInfo = DType.Color;

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

        public LogLevel LogLevel = LogLevel.Debug;
        public List<TagColor> ColorTags = new List<TagColor>();

        public bool OverrideSalvageGeneration = true;
        public bool NoLootCTDestroyed = false;
        public bool BaseECMValidation = true;

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
        public bool FixSaveGameMech = false;
        public bool TagRestrictionDropValidateRequiredTags = false;
        public bool TagRestrictionDropValidateIncompatibleTags = true;
        public bool CheckCriticalComponent = true;
        public bool UseDefaultFixedColor = false;
        public bool AddWeightToCategory = true;

        public UIColor DefaultFlagBackgroundColor = UIColor.UpgradeColor;
        public UIColor InvalidFlagBackgroundColor = UIColor.Red;

        public UIColor DefaultOverlayColor = UIColor.DarkGrayEighth;
        public CCColor PreinstalledOverlayCColor = new CCColor() { A = 12, R = 255, B = 180, G = 180 };
        public CCColor DefaultFlagOverlayCColor = new CCColor() { A = 12, R = 180, B = 180, G = 255 };
        public string CategoryDescriptionColor = "#008000";

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



        public string[] IgnoreValidationTags = null;
        public TagUnitType[] UnitTypes = null;

        public bool AllowMechlabWrongHardpoints = false;

        public ErrorMessages Message = new ErrorMessages();


        public void Complete()
        {
            PreinstalledOverlayColor = PreinstalledOverlayCColor.ToColor();
            DefaultFlagOverlayColor = DefaultFlagOverlayCColor.ToColor();

            ColorTagsDictionary = new Dictionary<string, Color>();
            if (ColorTags != null)
                foreach (var colorTag in ColorTags)
                {
                    if (!ColorTagsDictionary.ContainsKey(colorTag.Tag))
                        ColorTagsDictionary.Add(colorTag.Tag, colorTag.ToColor());
                }
        }
    }
}
