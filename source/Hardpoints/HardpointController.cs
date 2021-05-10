using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;
using HBS;
using Localize;
using Harmony;

namespace CustomComponents
{
    public class HardpointController
    {
        private static HardpointController _instance;

        public static HardpointController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HardpointController();

                return _instance;
            }
        }

        private Dictionary<string, HardpointInfo> Hardpoints { get; set; } = new Dictionary<string, HardpointInfo>();
        public List<HardpointInfo> HardpointsList { get; private set; }

        public void SetupDefaults()
        {
            var hp = new HardpointInfo()
            {
                ID = "Ballistic",
                Short = "B",
                DisplayName = "Ballistic",
                Visible = true,
                AcceptOmni = true
            };
            hp.Complete();
            Hardpoints["Ballistic"] = hp;

            hp = new HardpointInfo()
            {
                ID = "Energy",
                Short = "E",
                DisplayName = "Energy",
                Visible = true,
                AcceptOmni = true
            };
            hp.Complete();
            Hardpoints["Energy"] = hp;

            hp = new HardpointInfo()
            {
                ID = "Missile",
                Short = "M",
                DisplayName = "Missile",
                Visible = true,
                AcceptOmni = true
            };
            hp.Complete();
            Hardpoints["Missile"] = hp;

            hp = new HardpointInfo()
            {
                ID = "AntiPersonnel",
                Short = "S",
                DisplayName = "Support",
                Visible = true,
                AcceptOmni = true
            };
            hp.Complete();
            Hardpoints["AntiPersonnel"] = hp;

            hp = new HardpointInfo()
            {
                ID = "AMS",
                Short = "AM",
                DisplayName = "AMS",
                Visible = false,
                AcceptOmni = false
            };
            hp.Complete();
            Hardpoints["AMS"] = hp;

            hp = new HardpointInfo()
            {
                ID = "Melee",
                Short = "Ml",
                DisplayName = "Melee",
                Visible = false,
                AcceptOmni = false
            };
            hp.Complete();
            Hardpoints["Melee"] = hp;

        }

        public HardpointInfo this[WeaponCategoryValue wc] => wc == null ? null : this[wc.Name];

        public HardpointInfo this[string wcname]
        {
            get
            {
                if (Hardpoints.TryGetValue(wcname, out var result))
                    return result;
                Control.LogError($"{wcname} - dont have weapon category info!");
                return null;
            }

        }
        public void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            SetupDefaults();

            foreach (var hp in SettingsResourcesTools.Enumerate<HardpointInfo>("CCHardpoints", customResources))
            {
                if (hp.Complete())
                {
                    Control.LogDebug(DType.Hardpoints, $"Hardpoint info: {hp.ID}, [{hp.Compatible.Aggregate((last, next) => last + " " + next)}]");
                    Hardpoints[hp.ID] = hp;
                }
            }

            HardpointsList = Hardpoints.Values.OrderBy(i => i.Compatible.Length).ToList();
        }


        public string PostValidatorDrop(MechLabItemSlotElement drop_item, List<InvItem> new_inventory)
        {
            if (Control.Settings.AllowMechlabWrongHardpoints)
                return string.Empty;

            var mechDef = MechLabHelper.CurrentMechLab.ActiveMech;


            var weapons_per_location = new_inventory
                .Select(i => new { location = i.Location, item = i.Item, wcat = i.Item.GetWeaponCategory() })
                .Where(i => !i.wcat.Is_NotSet)

                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, items = i.ToList() })
                .ToList();

            foreach (var w_location in weapons_per_location)
            {
                var (omni, hardpoints) = GetHardpointsPerLocation(mechDef, w_location.location);

                foreach (var recrd in w_location.items)
                {
                    if (Hardpoints.TryGetValue(recrd.wcat.Name, out var hpInfo))
                    {
                        var nearest = hardpoints.FirstOrDefault(i => i.Compatible.Contains(hpInfo.ID));
                        if (nearest != null || hpInfo.AcceptOmni && omni > 0)
                        {
                            if (nearest == null)
                                omni -= 1;
                            else
                                hardpoints.Remove(nearest);
                        }
                        else
                            return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                                  mechDef.Description.UIName, recrd.item.Def.Description.Name, recrd.item.Def.Description.UIName,
                                  recrd.wcat.Name, recrd.wcat.FriendlyName,
                                  w_location.location
                              ).ToString();

                    }
                }
            }

            return string.Empty;
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationlevel, MechDef mechdef)
        {
            var weapons_per_location = mechdef.Inventory
                .Select(i => new { location = i.MountedLocation, item = i, wcat = i.GetWeaponCategory() })
                .Where(i => !i.wcat.Is_NotSet)

                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, items = i.ToList() })
                .ToList();


            foreach (var w_location in weapons_per_location)
            {
                var (omni, hardpoints) = GetHardpointsPerLocation(mechdef, w_location.location);

                foreach (var recrd in w_location.items)
                {
                    if (Hardpoints.TryGetValue(recrd.wcat.Name, out var hpInfo))
                    {
                        var nearest = hardpoints.FirstOrDefault(i => i.Compatible.Contains(hpInfo.ID));
                        if (nearest != null || hpInfo.AcceptOmni && omni > 0)
                        {
                            if (nearest == null)
                                omni -= 1;
                            else
                                hardpoints.Remove(nearest);
                        }
                        else
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(
                                Control.Settings.Message.Base_AddNotEnoughHardpoints,
                                mechdef.Description.UIName, recrd.item.Def.Description.Name,
                                recrd.item.Def.Description.UIName,
                                recrd.wcat.Name, recrd.wcat.FriendlyName,
                                w_location.location
                            ));

                    }
                }
            }
        }

        internal bool CanBeFielded(MechDef mechDef)
        {
            var weapons_per_location = mechDef.Inventory
                .Select(i => new { location = i.MountedLocation, item = i, wcat = i.GetWeaponCategory() })
                .Where(i => !i.wcat.Is_NotSet)

                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, items = i.ToList() })
                .ToList();


            foreach (var w_location in weapons_per_location)
            {
                var (omni, hardpoints) = GetHardpointsPerLocation(mechDef, w_location.location);

                foreach (var recrd in w_location.items)
                {
                    if (Hardpoints.TryGetValue(recrd.wcat.Name, out var hpInfo))
                    {
                        var nearest = hardpoints.FirstOrDefault(i => i.Compatible.Contains(hpInfo.ID));
                        if (nearest != null || hpInfo.AcceptOmni && omni > 0)
                        {
                            if (nearest == null)
                                omni -= 1;
                            else
                                hardpoints.Remove(nearest);
                        }
                        else
                            return false;

                    }
                }
            }

            return true;
        }

        public string ReplaceValidatorDrop(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {
            var use_hp = drop_item.ComponentRef.Def.GetComponent<UseHardpointCustom>();

            if (use_hp == null || use_hp.WeaponCategory.Is_NotSet || use_hp.hpInfo == null)
                return string.Empty;

            var hpinfo = use_hp.hpInfo;

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

                if (slotitem.ComponentRef.Is<UseHardpointCustom>(out var oth_use_hp))
                    continue;

                var hardpoint = hardpoints.FirstOrDefault(i => i.Compatible.Contains(oth_use_hp.WeaponCategory.Name));

                if (hardpoint != null)
                {
                    hardpoints.Remove(hardpoint);
                    if (hardpoint.Compatible.Contains(use_hp.WeaponCategory.Name))
                    {
                        candidants.Add(slotitem);
                    }
                }
                else if (Hardpoints[use_hp.WeaponCategory.Name].AcceptOmni && omni > 0)
                {
                    omni--;
                    if (hpinfo.AcceptOmni)
                        candidants.Add(slotitem);
                }
                else if (!Control.Settings.AllowMechlabWrongHardpoints)
                {
                    return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                        MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, drop_item.ComponentRef.Def.Description.Name,
                        drop_item.ComponentRef.Def.Description.UIName, hpinfo.DisplayName, use_hp.WeaponCategory.FriendlyName,
                        location
                    ).ToString();
                }
            }

            if (hardpoints.Count > 0 && hardpoints.Any(i => i.Compatible.Contains(hpinfo.ID)))
                return string.Empty;
            if (hpinfo.AcceptOmni && omni > 0)
                return string.Empty;

            candidants.RemoveAll(i => i.ComponentRef.IsFixed);

            if (candidants.Count == 0)
            {
                return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                    MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, drop_item.ComponentRef.Def.Description.Name,
                    drop_item.ComponentRef.Def.Description.UIName, use_hp.WeaponCategory.Name, use_hp.WeaponCategory.FriendlyName,
                    location
                ).ToString();
            }

            var toremove = candidants
                .OrderBy(i => i.ComponentRef.Def.InventorySize)
                .First();
            changes.Enqueue(new Change_Remove(toremove.ComponentRef.ComponentDefID, location));
            return string.Empty;
        }

        private (int, List<HardpointInfo>) GetHardpointsPerLocation(MechDef mechDef, ChassisLocations location)
        {
            var locationdef = mechDef.GetChassisLocationDef(location);
            int omni = 0;
            var hpinfos = new List<HardpointInfo>();
            foreach (var hp in locationdef.Hardpoints)
            {
                if (hp.Omni)
                    omni += 1;
                else if (Hardpoints.TryGetValue(hp.WeaponMountValue.Name, out var hpinfo))
                    hpinfos.Add(hpinfo);
                else
                    Control.LogError($"Unknown Hardpoint type {hp.WeaponMountValue.Name} for {mechDef.ChassisID}");
            }

            hpinfos.Sort((a, b) => a.Compatible.Length.CompareTo(b.Compatible.Length));

            return (omni, hpinfos);
        }
    }
}