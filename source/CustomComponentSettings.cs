using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using HBS.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomComponents
{
    [SerializeField]
    public class DefaultsInfo : IDefault
    {
        public string Tag { get; set; }

        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;
        public bool AddIfNotPresent { get; set; } = true;

        public MechComponentRef GetReplace(MechDef mechDef, SimGameState state)
        {
            var res = DefaultHelper.CreateRef(DefID, Type, mechDef.DataManager, state);
            res.SetData(Location, -1, ComponentDamageLevel.Functional, true);
            return res;
        }

        public virtual bool AddItems(MechDef mechDef, SimGameState state)
        {
            if (AddIfNotPresent)
            {
                DefaultHelper.AddInventory(DefID, mechDef, Location, Type, state);
                return true;
            }
            return false;
        }

        public bool NeedReplaceExistDefault(MechDef mechDef, MechComponentRef item)
        {
            return item.ComponentDefID != DefID;
        }

        public override string ToString()
        {
            return "DefaultsInfo: " + DefID;
        }
    }



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
        EffectNull = 1 << 1,
        ShowConfig = 1 << 2,
        ComponentInstall = 1 << 3,
        MechValidation = 1 << 4,
        CCLoading = 1 << 5,
        CCLoadingSummary = 1 << 6,
        InstallCost = 1 << 7,
        FixedCheck = 1 << 8,
        DefaultHandle = 1 << 9,
        ClearInventory = 1 << 10,
        CustomResource = 1 << 11,
    }


    public class CustomComponentSettings
    {
        public DType DebugInfo = DType.EffectNull | DType.SalvageProccess | DType.ComponentInstall;

        public LogLevel LogLevel = LogLevel.Debug;
        public List<TagColor> ColorTags = new List<TagColor>();

        public bool OverrideSalvageGeneration = true;
        public bool NoLootCTDestroyed = false;

        public bool OverrideRecoveryChance = true;
        public bool SalvageUnrecoveredMech = true;
        public float BaseRecoveryChance = 0.6f;
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

        public bool UseDefaultFixedColor = false;
        public UIColor DefaultFlagBackgroundColor = UIColor.UpgradeColor;
        public UIColor InvalidFlagBackgroundColor = UIColor.Red;

        public CCColor PreinstalledOverlayCColor = new CCColor() { A = 12, R = 255, B = 180, G = 180 };
        public CCColor DefaultFlagOverlayCColor = new CCColor() { A = 12, R = 180, B = 180, G = 255 };
        public string CategoryDescriptionColor = "#008000";

        [JsonIgnore] public Color PreinstalledOverlayColor;
        [JsonIgnore] public Color DefaultFlagOverlayColor;
        [JsonIgnore] public Dictionary<string, Color> ColorTagsDictionary;

        public bool TestEnableAllTags = false;
        public string OmniTechFlag = "cc_omnitech";
        public bool OmniTechCostBySize = false;
        public int OmniTechInstallCost = 1;

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
