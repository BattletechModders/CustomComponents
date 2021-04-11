using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using HBS;
using Localize;

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

        public Dictionary<string, HardpointInfo> Hardpoints { get; private set; } = new Dictionary<string, HardpointInfo>();
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

        public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
        {
            if (item.ComponentRef.ComponentDefType != ComponentType.Weapon)
                return string.Empty;
            var weapon = item.ComponentRef.Def as WeaponDef;


            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);
            if (lhepler.OmniHardpoints == 0 ||
                lhepler.Hardpoints.All(i => !i.Compatible.Contains(weapon.WeaponCategoryValue.Name)))
            {
                var mech = MechLabHelper.CurrentMechLab.ActiveMech;
                return new Localize.Text(Control.Settings.Message.Base_AddNoHardpoins, mech.Description.UIName,
                    weapon.Description.Name, weapon.Description.UIName, weapon.WeaponCategoryValue.Name, weapon.WeaponCategoryValue.FriendlyName,
                    location
                    ).ToString();
            }

            return string.Empty;
        }

        public string ReplaceValidatorDrop(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {
            if (drop_item.ComponentRef.ComponentDefType != ComponentType.Weapon)
                return string.Empty;

            var weapon = drop_item.ComponentRef.Def as WeaponDef;

            if (!Hardpoints.TryGetValue(weapon.WeaponCategoryValue.Name, out var hpinfo))
            {
                Control.LogError($"Invalid weapon category {weapon.WeaponCategoryValue.Name} for {weapon.Description.Id}");
                return "INVALID WEAPON, REPORT TO DISCORD!";
            }

            var removed = changes
                .OfType<RemoveChange>()
                .Where(i => i.item.ComponentRef.ComponentDefType == ComponentType.Weapon && i.location == location)
                .Select(i => new { weapon = (i.item.ComponentRef.Def as WeaponDef), slot = i.item })
                .ToArray();

            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

            var hardpoints = lhepler.Hardpoints.ToList();
            var omni = lhepler.OmniHardpoints;
            var candidants = new List<MechLabItemSlotElement>();

            foreach (var slotitem in lhepler.LocalInventory.Where(i => i.ComponentRef.ComponentDefType == ComponentType.Weapon))
            {
                if(removed.Any(i => i.slot == slotitem))
                    continue;

                var w = slotitem.ComponentRef.Def as WeaponDef;

                var hardpoint = hardpoints.FirstOrDefault(i => i.Compatible.Contains(w.WeaponCategoryValue.Name));

                if (hardpoint != null)
                {
                    hardpoints.Remove(hardpoint);
                    if (hardpoint.Compatible.Contains(weapon.WeaponCategoryValue.Name))
                    {
                        candidants.Add(slotitem);
                    }
                }
                else if (Hardpoints[weapon.WeaponCategoryValue.Name].AcceptOmni && omni > 0)
                {
                    omni--;
                    if(hpinfo.AcceptOmni)
                        candidants.Add(slotitem);
                }
                else if(!Control.Settings.AllowMechlabWrongHardpoints)
                {
                    return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                        MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, weapon.Description.Name, weapon.Description.UIName,
                        hpinfo.DisplayName, weapon.WeaponCategoryValue.FriendlyName,
                        location
                    ).ToString();
                }
            }

            if (hardpoints.Count > 0 && hardpoints.Any(i => i.Compatible.Contains(hpinfo.ID)))
                return string.Empty;
            else if (hpinfo.AcceptOmni && omni > 0)
                return string.Empty;

            candidants.RemoveAll(i => i.ComponentRef.IsDefault() && i.ComponentRef.HasFlag(CCF.NoRemove));
            if (candidants.Count == 0)
            {
                return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                    MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, weapon.Description.Name, weapon.Description.UIName,
                    hpinfo.DisplayName, weapon.WeaponCategoryValue.FriendlyName,
                    location
                ).ToString();
            }

            var toremove = candidants
                .OrderBy(i => i.ComponentRef.IsDefault() ? 0 : 20 + i.ComponentRef.Def.InventorySize)
                .First();
            changes.Enqueue(new RemoveChange(location, toremove));

            return string.Empty;
        }

        public string PostValidatorDrop(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
            if (Control.Settings.AllowMechlabWrongHardpoints)
                return string.Empty;

            var weapons_per_location = new_inventory
                .Where(i => i.item.ComponentDefType == ComponentType.Weapon)
                .Select(i => new { location = i.location, weapon = i.item.Def as WeaponDef })
                .Where(i => i.weapon != null)

                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, items = i.Select(i => i.weapon).ToList() })
                .ToList();

            var mechdef = MechLabHelper.CurrentMechLab.ActiveMech;

            foreach (var w_location in weapons_per_location)
            {
                var (omni, hardpoints) = GetHardpointsPerLocation(mechdef, w_location.location);

                foreach (var item in w_location.items)
                {
                    if (Hardpoints.TryGetValue(item.WeaponCategoryValue.Name, out var hpInfo))
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
                          return new Localize.Text(Control.Settings.Message.Base_ValidateNotEnoughHardpoints,
                                mechdef.Description.UIName, item.Description.Name, item.Description.UIName,
                                item.WeaponCategoryValue.Name, item.WeaponCategoryValue.FriendlyName,
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
                .Where(i => i.ComponentDefType == ComponentType.Weapon)
                .Select(i => new { location = i.MountedLocation, weapon = i.Def as WeaponDef })
                .Where(i => i.weapon != null)

                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, items = i.Select(i => i.weapon).ToList() })
                .ToList();

            foreach (var w_location in weapons_per_location)
            {
                var (omni, hardpoints) = GetHardpointsPerLocation(mechdef, w_location.location);

                foreach (var item in w_location.items)
                {
                    if (Hardpoints.TryGetValue(item.WeaponCategoryValue.Name, out var hpInfo))
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
                            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(Control.Settings.Message.Base_ValidateNotEnoughHardpoints,
                                mechdef.Description.UIName, item.Description.Name, item.Description.UIName,
                                item.WeaponCategoryValue.Name, item.WeaponCategoryValue.FriendlyName,
                                w_location.location
                            ));

                    }
                }
            }
        }

        internal bool CanBeFielded(MechDef mechDef)
        {
            var weapons_per_location = mechDef.Inventory
                .Where(i => i.ComponentDefType == ComponentType.Weapon)
                .Select(i => new { location = i.MountedLocation, weapon = i.Def as WeaponDef })
                .Where(i => i.weapon != null)

                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, items = i.Select(i => i.weapon).ToList() })
                .ToList();

            foreach (var w_location in weapons_per_location)
            {
                var (omni, hardpoints) = GetHardpointsPerLocation(mechDef, w_location.location);

                foreach (var item in w_location.items)
                {
                    if (Hardpoints.TryGetValue(item.WeaponCategoryValue.Name, out var hpInfo))
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