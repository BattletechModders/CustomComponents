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

            var weapons_per_location = new_inventory
                .Where(i => i.Item.ComponentDefType == ComponentType.Weapon)
                .Select(i => new { location = i.Location, weapon = i.Item.Def as WeaponDef })
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
                            return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
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