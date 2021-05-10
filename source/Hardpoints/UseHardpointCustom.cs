using System;
using System.CodeDom;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.ExtendedDetails;
using Harmony;
using JetBrains.Annotations;

namespace CustomComponents
{
    [CustomComponent("UseHardpoint")]
    public class UseHardpointCustom : SimpleCustomComponent, IValueComponent, IOnAdd, IOnRemove, IAdjustDescription, IPreValidateDrop
    {
        public WeaponCategoryValue WeaponCategory { get; private set; } = WeaponCategoryEnumeration.GetNotSetValue();
        public HardpointInfo hpInfo { get; private set; }

        public void LoadValue(object value)
        {
            Control.LogDebug(DType.Hardpoints, $"UseHardpoint LoadValue for {Def.Description.Id}: {value}");

            var cat = WeaponCategoryEnumeration.GetWeaponCategoryByName(value.ToString());
            WeaponCategory = cat ?? WeaponCategoryEnumeration.GetNotSetValue();

            hpInfo = WeaponCategory.Is_NotSet ? null : HardpointController.Instance[WeaponCategory];

            Control.LogDebug(DType.Hardpoints, $"- {cat.WeaponCategoryID}:{cat.Name}/{cat.FriendlyName}");
        }

        internal void SetValue(object value)
        {
            var str = value.ToString();
            var WeaponCategory = WeaponCategoryEnumeration.GetWeaponCategoryByName(str);
            hpInfo = HardpointController.Instance[str];
        }

        public void OnAdd(ChassisLocations location, InventoryOperationState state)
        {
            if (!Def.IsDefault() && state.Mech.HasWeaponDefaults())
                state.AddChange(new Change_WeaponAdjust());
        }

        public void OnRemove(ChassisLocations location, InventoryOperationState state)
        {
            if (!Def.IsDefault() && state.Mech.HasWeaponDefaults())
                state.AddChange(new Change_WeaponAdjust());
        }

        public void AdjustDescription()
        {
            if (WeaponCategory.Is_NotSet)
                return;

            var hpinfo = HardpointController.Instance[WeaponCategory];
            if (hpinfo == null || !hpinfo.Visible)
                return;

            var ed = ExtendedDetails.ExtendedDetails.GetOrCreate(Def);
            var detail =
                ed.GetDetails().FirstOrDefault(i => i.Identifier == "Hardpoints") as
                    ExtendedDetails.ExtendedDetailList ??
                new ExtendedDetailList()
                {
                    Index = 10,
                    Identifier = "Hardpoints",
                    OpenBracket = $"\n<b>Using Hardpoints: <color={Control.Settings.HardpointDescriptionColor}>[",
                    CloseBracket = "]</color></b>\n"
                };

            detail.AddUnique(WeaponCategory.FriendlyName);
            ed.AddDetail(detail);
        }

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            if (WeaponCategory.Is_NotSet)
                return string.Empty;

            Control.LogDebug(DType.Hardpoints, $"PreValidateDrop {Def.Description.Id}[{WeaponCategory.Name}]");

            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

            if ((!hpInfo.AcceptOmni || lhepler.OmniHardpoints == 0) &&
                lhepler.Hardpoints.All(i => !i.Compatible.Contains(WeaponCategory.Name)))
            {
                if (Control.Settings.DebugInfo.HasFlag(DType.Hardpoints))
                {
                    Control.LogDebug(DType.Hardpoints, $"- omni: {lhepler.OmniHardpoints}");
                    foreach (var hp in lhepler.Hardpoints)
                    {
                        Control.LogDebug(DType.Hardpoints,
                            $"- id:{hp.ID} omni:{hp.AcceptOmni} comp:{hp.Compatible.Join()}");
                    }
                }

                var mech = MechLabHelper.CurrentMechLab.ActiveMech;
                return new Localize.Text(Control.Settings.Message.Base_AddNoHardpoins, mech.Description.UIName,
                    Def.Description.Name, Def.Description.UIName, WeaponCategory.Name, WeaponCategory.FriendlyName,
                    location
                ).ToString();
            }

            return string.Empty;
        }
    }
}
