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

            Control.LogDebug(DType.Hardpoints, $"PreValidateDrop {weapon.Description.Id}[{weapon.WeaponCategoryValue.Name}]");

            var lhepler = MechLabHelper.CurrentMechLab.GetLocationHelper(location);
            if (lhepler.OmniHardpoints == 0 &&
                lhepler.Hardpoints.All(i => !i.Compatible.Contains(weapon.WeaponCategoryValue.Name)))
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
                    weapon.Description.Name, weapon.Description.UIName, weapon.WeaponCategoryValue.Name, weapon.WeaponCategoryValue.FriendlyName,
                    location
                    ).ToString();
            }

            return string.Empty;
        }

        public string PostValidatorDrop(MechLabItemSlotElement drop_item, List<InvItem> new_inventory)
        {
            if (Control.Settings.AllowMechlabWrongHardpoints)
                return string.Empty;

            var mechDef = MechLabHelper.CurrentMechLab.ActiveMech;


            var weapons_per_location = new_inventory
                .Select(i => new { location = i.Location, item = i.Item, wcat = i.Item.GetWeaponCategory() })
                .Where(i => !i.wcat.Is_NotSet )

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
            if (drop_item.ComponentRef.ComponentDefType != ComponentType.Weapon)
                return string.Empty;

            var weapon = drop_item.ComponentRef.Def as WeaponDef;

            if (!Hardpoints.TryGetValue(weapon.WeaponCategoryValue.Name, out var hpinfo))
            {
                Control.LogError($"Invalid weapon category {weapon.WeaponCategoryValue.Name} for {weapon.Description.Id}");
                return "INVALID WEAPON, REPORT TO DISCORD!";
            }

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
                    ;
                }

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
                    if (hpinfo.AcceptOmni)
                        candidants.Add(slotitem);
                }
                else if (!Control.Settings.AllowMechlabWrongHardpoints)
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
            if (hpinfo.AcceptOmni && omni > 0)
                return string.Empty;

            candidants.RemoveAll(i => i.ComponentRef.IsFixed);

            if (candidants.Count == 0)
            {
                return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                    MechLabHelper.CurrentMechLab.ActiveMech.Description.UIName, weapon.Description.Name, weapon.Description.UIName,
                    hpinfo.DisplayName, weapon.WeaponCategoryValue.FriendlyName,
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