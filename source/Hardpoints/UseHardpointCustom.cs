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
        IPreValidateDrop, IReplaceValidateDrop, IValid
    {
        public WeaponCategoryValue WeaponCategory { get; private set; } = WeaponCategoryEnumeration.GetNotSetValue();
        public HardpointInfo hpInfo { get; private set; }

        public void LoadValue(string value)
        {
            Control.LogDebug(DType.Hardpoints, $"UseHardpoint LoadValue for {Def.Description.Id}: {value}");

            var cat = WeaponCategoryEnumeration.GetWeaponCategoryByName(value);
            WeaponCategory = cat ?? WeaponCategoryEnumeration.GetNotSetValue();

            hpInfo = WeaponCategory.Is_NotSet ? null : HardpointController.Instance[WeaponCategory];
            if (hpInfo != null && !hpInfo.AllowOnWeapon)
            {
                Control.LogError($"{Def.Description.Id} use {value} weapon category that cannot be used on weapons");
                hpInfo = null;
                WeaponCategory = WeaponCategoryEnumeration.GetNotSetValue();
            }

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

            ExtendedDetails.ExtendedDetails.GetOrCreate(Def).AddIfMissing(
                new ExtendedDetail
                {
                    Index = Control.Settings.HardpointDescriptionIndex,
                    Identifier = "UseHardpoints",
                    Text = "\n\nRequired Hardpoint: <b><color="+ Control.Settings.HardpointDescriptionColor + ">"
                           + WeaponCategory.FriendlyName
                           + "</color></b>"
                }
            );
        }

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            if (WeaponCategory.Is_NotSet)
                return string.Empty;

            Control.LogDebug(DType.Hardpoints, $"PreValidateDrop {Def.Description.Id}[{WeaponCategory.Name}]");

            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);
            var hp = lhepler.HardpointsUsage;


            if (lhepler.HardpointsUsage.All(i => !i.hpInfo.CompatibleID.Contains(WeaponCategory.ID)))
            {
                var mech = MechLabHelper.CurrentMechLab.ActiveMech;
                return new Localize.Text(Control.Settings.Message.Base_AddNoHardpoints, mech.Description.UIName,
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

            var hardpoints = lhepler.HardpointsUsage.Select(i => new HPUsage(i, true)).ToList();
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

                var hardpoint = hardpoints.FirstOrDefault(i => i.Used < i.Total && i.hpInfo.CompatibleID.Contains(oth_use_hp.WeaponCategory.ID));

                if (hardpoint != null)
                {
                    hardpoint.Used +=1;
                    if (hardpoint.hpInfo.CompatibleID.Contains(WeaponCategory.ID))
                    {
                        candidants.Add(slotitem);
                    }
                }
                else if (!Control.Settings.AllowMechlabWrongHardpoints)
                {
                    return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                        MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, drop_item.ComponentRef.Def.Description.Name,
                        drop_item.ComponentRef.Def.Description.UIName, WeaponCategory.Name, WeaponCategory.FriendlyName,
                        location
                    ).ToString();
                }
            }

            if (hardpoints.Any(i => i.Used < i.Total && i.hpInfo.CompatibleID.Contains(hpInfo.WeaponCategory.ID)))
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

        public bool Valid => hpInfo != null;
    }
}
