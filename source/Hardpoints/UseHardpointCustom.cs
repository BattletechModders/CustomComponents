using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;
using CustomComponents.ExtendedDetails;
using Harmony;
using JetBrains.Annotations;

namespace CustomComponents
{
    [CustomComponent("UseHardpoint")]
    public class UseHardpointCustom : SimpleCustomComponent, IValueComponent<string>, IOnAdd, IOnRemove, IAdjustDescription,
        IPreValidateDrop, IReplaceValidateDrop
    {
        public WeaponCategoryValue WeaponCategory { get; private set; } = WeaponCategoryEnumeration.GetNotSetValue();
        public HardpointInfo hpInfo { get; private set; }

        public void LoadValue(string value)
        {
            Control.LogDebug(DType.Hardpoints, $"UseHardpoint LoadValue for {Def.Description.Id}: {value}");

            var cat = WeaponCategoryEnumeration.GetWeaponCategoryByName(value);
            WeaponCategory = cat ?? WeaponCategoryEnumeration.GetNotSetValue();

            hpInfo = WeaponCategory.Is_NotSet ? null : HardpointController.Instance[WeaponCategory];

            Control.LogDebug(DType.Hardpoints, $"- {cat.WeaponCategoryID}:{cat.Name}/{cat.FriendlyName}");
        }

        public void OnAdd(ChassisLocations location, InventoryOperationState state)
        {
            if (!Def.IsDefault() && state.Mech.HasWeaponDefaults(location))
            {
                state.AddChange(new Change_WeaponAdjust(location));
            }
        }

        public void OnRemove(ChassisLocations location, InventoryOperationState state)
        {
            if (!Def.IsDefault() && state.Mech.HasWeaponDefaults(location))
                state.AddChange(new Change_WeaponAdjust(location));
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

        public string ReplaceValidateDrop(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {

            if (WeaponCategory.Is_NotSet || hpInfo == null)
                return string.Empty;


            var removed = changes.OfType<Change_Remove>().ToList();

            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

            var hardpoints = lhepler.Hardpoints.ToList();
            var omni = lhepler.OmniHardpoints;
            var candidants = new List<MechLabItemSlotElement>();

            foreach (var slotitem in lhepler.LocalInventory.Where(i => i.ComponentRef.ComponentDefType == ComponentType.Weapon))
            {

                var already_removed = removed.FirstOrDefault(i =>
                    i.ItemID == slotitem.ComponentRef.ComponentDefID && i.Location == slotitem.MountedLocation);

                if (already_removed != null)
                {
                    removed.Remove(already_removed);
                    continue;
                }

                if (!slotitem.ComponentRef.Is<UseHardpointCustom>(out var oth_use_hp))
                    continue;

                if (slotitem.ComponentRef.IsDefault() && !slotitem.ComponentRef.IsModuleFixed(MechLabHelper.CurrentMechLab.ActiveMech))
                    continue;

                var hardpoint = hardpoints.FirstOrDefault(i => i.Compatible.Contains(oth_use_hp.WeaponCategory.Name));

                if (hardpoint != null)
                {
                    hardpoints.Remove(hardpoint);
                    if (hardpoint.Compatible.Contains(WeaponCategory.Name))
                    {
                        candidants.Add(slotitem);
                    }
                }
                else if (hpInfo.AcceptOmni && omni > 0)
                {
                    omni--;
                    if (hpInfo.AcceptOmni)
                        candidants.Add(slotitem);
                }
                else if (!Control.Settings.AllowMechlabWrongHardpoints)
                {
                    return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                        MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, drop_item.ComponentRef.Def.Description.Name,
                        drop_item.ComponentRef.Def.Description.UIName, hpInfo.DisplayName, WeaponCategory.FriendlyName,
                        location
                    ).ToString();
                }
            }

            if (hardpoints.Count > 0 && hardpoints.Any(i => i.Compatible.Contains(hpInfo.ID)))
                return string.Empty;
            if (hpInfo.AcceptOmni && omni > 0)
                return string.Empty;

            candidants.RemoveAll(i => i.ComponentRef.IsFixed);

            if (candidants.Count == 0)
            {
                return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                    MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, drop_item.ComponentRef.Def.Description.Name,
                    drop_item.ComponentRef.Def.Description.UIName, WeaponCategory.Name, WeaponCategory.FriendlyName,
                    location
                ).ToString();
            }

            var toremove = candidants
                .OrderBy(i => i.ComponentRef.Def.InventorySize)
                .First();
            changes.Enqueue(new Change_Remove(toremove.ComponentRef.ComponentDefID, location));
            return string.Empty;
        }
    }
}
