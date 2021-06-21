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

        private Dictionary<string, HardpointInfo> HardpointsByName { get; set; } = new Dictionary<string, HardpointInfo>();
        private Dictionary<int, HardpointInfo> HardpointsByID { get; set; } = new Dictionary<int, HardpointInfo>();


        public List<HardpointInfo> HardpointsList { get; private set; }

        public void SetupDefaults()
        {
            var hp = new HardpointInfo()
            {
                ID = "Ballistic",
                Visible = true,
                AllowOnWeapon = true,
                OverrideColor = false,
            };
            hp.Complete();
            HardpointsByName[hp.ID] = hp;
            HardpointsByID[hp.WeaponCategory.ID] = hp;

            hp = new HardpointInfo()
            {
                ID = "Energy",
                Visible = true,
                AllowOnWeapon = true
            };
            hp.Complete();
            HardpointsByName[hp.ID] = hp;
            HardpointsByID[hp.WeaponCategory.ID] = hp;

            hp = new HardpointInfo()
            {
                ID = "Missile",
                Visible = true,
                AllowOnWeapon = true
            };
            hp.Complete();
            HardpointsByName[hp.ID] = hp;
            HardpointsByID[hp.WeaponCategory.ID] = hp;

            hp = new HardpointInfo()
            {
                ID = "AntiPersonnel",
                Visible = true,
                AllowOnWeapon = true
            };
            hp.Complete();
            HardpointsByName[hp.ID] = hp;
            HardpointsByID[hp.WeaponCategory.ID] = hp;

            hp = new HardpointInfo()
            {
                ID = "AMS",
                Visible = false,
                AllowOnWeapon = true,
                AllowOmni = false
            };
            hp.Complete();
            HardpointsByName[hp.ID] = hp;
            HardpointsByID[hp.WeaponCategory.ID] = hp;

            hp = new HardpointInfo()
            {
                ID = "Melee",
                Visible = false,
                AllowOnWeapon = true,
                AllowOmni = false
            };
            hp.Complete();
            HardpointsByName[hp.ID] = hp;
            HardpointsByID[hp.WeaponCategory.ID] = hp;

        }

        public HardpointInfo this[WeaponCategoryValue wc] => wc == null ? null : this[wc.Name];

        public HardpointInfo this[string wcname]
        {
            get
            {
                if (HardpointsByName.TryGetValue(wcname, out var result))
                    return result;
                Control.LogError($"{wcname} - dont have weapon category info!");
                return null;
            }

        }

        public HardpointInfo this[int wcid]
        {
            get
            {
                if (HardpointsByID.TryGetValue(wcid, out var result))
                    return result;
                Control.LogError($"{wcid} - dont have weapon category info!");
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
                    if(Control.Settings.DEBUG_ShowLoadedHardpoints)
                        Control.Log($"Hardpoint {hp.ID} loaded, [{hp.CompatibleID.Aggregate("", (last, next) => last + " " + WeaponCategoryEnumeration.GetWeaponCategoryByID(next).FriendlyName)}]");
                    HardpointsByName[hp.ID] = hp;
                    HardpointsByID[hp.WeaponCategory.ID] = hp;
                }
            }
            HardpointsList = HardpointsByName.Values.OrderBy(i => i.CompatibleID.Count).ToList();

            HardpointInfo omni;
            if (HardpointsByID.TryGetValue(Control.Settings.OmniCategoryID, out omni))
            {
                var list = HardpointsList
                    .Where(i => i.Visible && i.AllowOnWeapon)
                    .Select(i => new { name = i.ID, id = i.WeaponCategory.ID })
                    .ToArray();

                omni.CompatibleID = list.Select(i => i.id).ToHashSet();
            }

            if (Control.Settings.DEBUG_ShowLoadedHardpoints)
            {
                Control.Log($"Hardpoints: Total {HardpointsList?.Count ?? 0} Loaded");
                if(omni != null)
                    Control.Log($"- omni list [{omni.CompatibleID.Aggregate("", (last, next) => last + " " + WeaponCategoryEnumeration.GetWeaponCategoryByID(next).FriendlyName)}]");
                else
                    Control.Log("- no omni hardpoint definition load");
            }
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
                var hardpoints = mechDef.GetAllHardpoints(w_location.location, new_inventory);

                //foreach (var hardpoint in hardpoints)
                //{
                //    Control.Log($"{w_location.location} - {hardpoint.hpInfo.WeaponCategory.Name} - {hardpoint.Used}/{hardpoint.Total}");
                //}

                foreach (var recrd in w_location.items)
                {
                    if (HardpointsByID.TryGetValue(recrd.wcat.ID, out var hpInfo))
                    {
                        var nearest = hardpoints.FirstOrDefault(i => i.Total > i.Used && i.hpInfo.CompatibleID.Contains(hpInfo.WeaponCategory.ID));
                        if (nearest != null)
                            nearest.Used += 1;
                        else
                            return new Localize.Text(Control.Settings.Message.Base_AddNotEnoughHardpoints,
                                mechDef.Description.UIName, recrd.item.Def.Description.Name,
                                recrd.item.Def.Description.UIName,
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
                var hardpoints = mechdef.GetAllHardpoints(w_location.location, mechdef.Inventory.ToInvItems());

                foreach (var recrd in w_location.items)
                {
                    if (HardpointsByID.TryGetValue(recrd.wcat.ID, out var hpInfo))
                    {
                        var nearest = hardpoints.FirstOrDefault(i => i.Total> i.Used && i.hpInfo.CompatibleID.Contains(hpInfo.WeaponCategory.ID));
                        if (nearest != null)
                            nearest.Used += 1;
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
                var hardpoints = mechDef.GetAllHardpoints(w_location.location, mechDef.Inventory.ToInvItems());

                foreach (var recrd in w_location.items)
                {
                    if (HardpointsByID.TryGetValue(recrd.wcat.ID, out var hpInfo))
                    {
                        var nearest = hardpoints.FirstOrDefault(i => i.Total > i.Used && i.hpInfo.CompatibleID.Contains(hpInfo.WeaponCategory.ID));
                        if (nearest != null)
                            nearest.Used += 1;
                        else
                            return false;
                    }
                }
            }

            return true;
        }
        public void FixMechs(List<MechDef> mechdefs, SimGameState simgame)
        {
            foreach (var mechdef in mechdefs)
            {
                var defaults = mechdef.GetWeaponDefaults();
                if (defaults == null)
                    continue;

                var def_list = defaults.ToList();

                if (def_list.Count == 0)
                    continue;

                var changes = new Queue<IChange>();
                foreach (var weaponDefault in defaults)
                    changes.Enqueue(new Change_WeaponAdjust(weaponDefault.Location));

                var state = new InventoryOperationState(changes, mechdef);
                state.DoChanges();
                state.ApplyInventory();
            }
        }
    }
}