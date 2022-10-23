using System.Collections.Generic;
using System.Linq;
using BattleTech;
using FluffyUnderware.DevTools.Extensions;

namespace CustomComponents
{
    public static class HardpointExtentions
    {
        public enum SortOrder
        {
            None, ID, Usage, Totals
        }
        public class WeaponDefaultRecord
        {
            public MechComponentDef Def { get; set; }
            public WeaponCategoryValue WeaponCategory { get; set; }
            public HashSet<string> Categories { get; set; }
            public bool HaveCategories => Categories != null && Categories.Count > 0;

            public ChassisLocations Location { get; set; }

        }

        private static Dictionary<string, ChassisLocations> have_defaults = new Dictionary<string, ChassisLocations>();
        private static Dictionary<string, WeaponCategoryValue> categories = new Dictionary<string, WeaponCategoryValue>();
        private static Dictionary<string, List<WeaponDefaultRecord>> defaults = new Dictionary<string, List<WeaponDefaultRecord>>();
        private static Dictionary<string, Dictionary<ChassisLocations, List<HPUsage>>> hp_database = new Dictionary<string, Dictionary<ChassisLocations, List<HPUsage>>>();


        private static WeaponCategoryValue notSet = WeaponCategoryEnumeration.GetNotSetValue();
        public static bool HasWeaponDefaults(this MechDef mech, ChassisLocations location)
        {
            if (mech == null)
                return false;

            if (!have_defaults.TryGetValue(mech.ChassisID, out var result))
            {

                Control.LogDebug(DType.WeaponDefaults, $"Build Weapon Defaults for {mech.ChassisID}");
                result = ChassisLocations.None;

                var defs = mech.GetWeaponDefaults();
                if (defs != null)
                {
                    var list = defs.ToList();
                    foreach (var wd in list)
                    {
                        Control.LogDebug(DType.WeaponDefaults, $"-- add {wd.Location}");
                        result = result.Set(wd.Location);
                    }

                    Control.LogDebug(DType.WeaponDefaults, $"- complete - {result}");
                }
                else
                {
                    Control.LogDebug(DType.WeaponDefaults, $"- no defaults - {result}");
                }

                have_defaults[mech.ChassisID] = result;
            }

            Control.LogDebug(DType.WeaponDefaults, $"HasWeaponDefaults {result} - {location} - {result.HasFlag(location)}");

            return result.HasFlag(location);
        }
        public static bool HasWeaponDefaults(this ChassisDef chassis, ChassisLocations location)
        {
            if (chassis == null)
                return false;

            if (!have_defaults.TryGetValue(chassis.Description.Id, out var result))
            {
                result = ChassisLocations.None;

                var defs = chassis.GetWeaponDefaults();
                if (defs != null)
                {
                    var list = defs.ToList();
                    foreach (var wd in list)
                        result = result.Set(wd.Location);

                }
                have_defaults[chassis.Description.Id] = result;
            }
            return result.HasFlag(location);
        }

        private static List<WeaponDefaultRecord> BuildWeaponDefaults(ChassisDef chassis)
        {
            var components = GetWeaponDefaultComponents(chassis).ToList();
            if (components.Count == 0)
                return null;

            var result = new List<WeaponDefaultRecord>();

            foreach (var weaponDefault in components)
            {
                var def = DefaultHelper.GetComponentDef(weaponDefault.DefID, weaponDefault.Type);
                if (def == null)
                {
                    Control.LogError($"Unknown weapon default {weaponDefault.DefID}/{weaponDefault.Type}");
                    continue;
                }

                if (!def.IsDefault())
                {
                    Control.LogError($"Weapon default {weaponDefault.DefID} is not default");
                    continue;
                }

                var wc = def.GetWeaponCategory();
                if (wc == null || wc.Is_NotSet)
                {
                    Control.LogError($"Weapon default {weaponDefault.DefID} not a weapon/UseHardpoint");
                    continue;
                }

                var rec = new WeaponDefaultRecord()
                {
                    WeaponCategory = wc,
                    Def = def,
                    Categories = weaponDefault.ReplaceCategories?.ToHashSet(),
                    Location = weaponDefault.Location
                };
                result.Add(rec);
            }

            return result.Count > 0 ? result : null;
        }

        private static IEnumerable<IWeaponDefault> GetWeaponDefaultComponents(ChassisDef chassis)
        {
            return chassis.GetComponents<IWeaponDefault>();
        }

        public static IEnumerable<WeaponDefaultRecord> GetWeaponDefaults(this MechDef mech)
        {
            if (mech == null)
                return null;

            if (!defaults.TryGetValue(mech.ChassisID, out var result))
            {
                result = BuildWeaponDefaults(mech.Chassis);
                defaults[mech.ChassisID] = result;
            }

            return result;
        }
        public static IEnumerable<WeaponDefaultRecord> GetWeaponDefaults(this ChassisDef chassis)
        {
            if (chassis == null)
                return null;

            if (!defaults.TryGetValue(chassis.Description.Id, out var result))
            {
                result = BuildWeaponDefaults(chassis);
                defaults[chassis.Description.Id] = result;

            }

            return result;
        }

        public static List<HPUsage> GetHardpoints(this MechDef mech, ChassisLocations location)
        {
            return mech?.Chassis.GetHardpoints(location);
        }
        public static List<HPUsage> GetHardpoints(this ChassisDef chassis, ChassisLocations location)
        {
            if (chassis == null || !DefaultsDatabase.SingleLocations.Contains(location))
                return null;

            var id = chassis.Description.Id;
            if (!hp_database.TryGetValue(id, out var dictionary))
            {
                dictionary = new Dictionary<ChassisLocations, List<HPUsage>>();

                foreach (var chassisLocationse in DefaultsDatabase.SingleLocations)
                {
                    dictionary = BuildHardpoints(chassis);
                    hp_database[id] = dictionary;
                }

                hp_database[id] = dictionary;
            }

            return dictionary[location];
        }
        public static List<HPUsage> GetHardpoints(this MechDef mech, SortOrder sort = SortOrder.Usage)
        {
            return mech?.Chassis.GetHardpoints(sort);
        }

        public static List<HPUsage> GetHardpoints(this ChassisDef chassis, SortOrder sort = SortOrder.Usage)
        {
            if (chassis == null)
                return null;

            var result = new List<HPUsage>();
            foreach (var location in DefaultsDatabase.SingleLocations)
            {
                var usage = GetHardpoints(chassis, location);
                if (usage != null && usage.Count > 0)
                    foreach (var hpUsage in usage)
                        AddToList(result, hpUsage);
            }

            switch (sort)
            {
                case SortOrder.ID:
                    result.Sort((a, b) => a.WeaponCategoryID.CompareTo(b.WeaponCategoryID));
                    break;
                case SortOrder.Usage:
                    result.Sort();
                    break;
                case SortOrder.Totals:
                    result.Sort((a, b) => a.Total.CompareTo(b.Total));
                    break;
            }

            return result;
        }

        private static void AddToList(List<HPUsage> list, HPUsage hp)
        {
            var item = list.FirstOrDefault(i => i.WeaponCategoryID == hp.WeaponCategoryID);
            if (item != null)
                item.Total += hp.Total;
            else
                list.Add(new HPUsage(hp));
        }


        public static List<HPUsage> GetHardpointUsage(this MechDef mech, ChassisLocations location, IEnumerable<InvItem> inventory = null)
        {
            if (inventory == null)
                inventory = mech.Inventory.ToInvItems();
            var inv = inventory.ToList();

            var result = mech.GetAllHardpoints(location, inv);
            foreach (var item in inv.Where(i => i.Location == location)
                .Select(i => i.Item.GetComponent<UseHardpointCustom>())
                .Where(i => i != null && !i.WeaponCategory.Is_NotSet))
            {
                HPUsage first = null;
                bool found = false;

                for (int i = 0; i < result.Count; i++)
                {
                    var hp = result[i];

                    if (!hp.hpInfo.CompatibleID.Contains(item.WeaponCategory.ID))
                        continue;
                    if (hp.Used < hp.Total)
                    {
                        found = true;
                        hp.Used += 1;
                    }

                    first ??= hp;
                }

                if (!found)
                    if (first == null)
                        result.Add(new HPUsage(item.hpInfo, 0, -1));
                    else
                        first.Used += 1;
            }

            return result;
        }

        public static List<HPUsage> GetHardpointUsage(this MechDef mech, IEnumerable<InvItem> inventory = null)
        {
            var result = new List<HPUsage>();

            if (mech != null)
                foreach (var location in DefaultsDatabase.SingleLocations)
                {
                    var usage = mech.GetHardpointUsage(location, inventory);

                    if (usage != null)
                        foreach (var hpUsage in usage)
                        {
                            var item = result.FirstOrDefault(i => i.hpInfo.WeaponCategory.ID == hpUsage.WeaponCategoryID);
                            if (item == null)
                                result.Add(new HPUsage(hpUsage));
                            else
                            {
                                item.Total += hpUsage.Total;
                                item.Used += hpUsage.Used;
                            }
                        }
                }

            return result;
        }


        public static List<HPUsage> GetAllHardpoints(this MechDef mech, ChassisLocations location,
            IEnumerable<InvItem> inventory = null)
        {
            return mech?.Chassis.GetAllHardpoints(location, inventory ?? mech.Inventory.ToInvItems());
        }
        public static List<HPUsage> GetAllHardpoints(this ChassisDef chassis, ChassisLocations location,
            IEnumerable<InvItem> inventory)
        {
            if (chassis == null)
                return null;

            var result = chassis.GetHardpoints(location).Select(a => new HPUsage(a, true)).ToList();

            foreach (var invItem in inventory.Where(i => i.Location == location))
            {
                if (invItem.Item.Is<AddHardpoint>(out var add) && add.Valid)
                {
                    AddToList(result, add.WeaponCategory);
                }
                else if (invItem.Item.Is<ReplaceHardpoint>(out var replace) && replace.Valid)
                {
                    AddToList(result, replace.AddWeaponCategory);
                    SubFromList(result, replace.UseWeaponCategory);
                }
            }

            return result;
        }

        private static Dictionary<ChassisLocations, List<HPUsage>> BuildHardpoints(ChassisDef chassis)
        {
            var result = new Dictionary<ChassisLocations, List<HPUsage>>();

            foreach (var location in DefaultsDatabase.SingleLocations)
            {
                var list = new List<HPUsage>();
                var def = chassis.GetLocationDef(location);
                if (def.Hardpoints != null && def.Hardpoints.Length > 0)
                {
                    foreach (var hpDef in def.Hardpoints)
                    {
                        var wc = hpDef.WeaponMountValue;
                        if (hpDef.Omni)
                            wc = WeaponCategoryEnumeration.GetWeaponCategoryByID(Control.Settings.OmniCategoryID);
                        if (wc == null || wc.Is_NotSet)
                            continue;

                        AddToList(list, wc);
                    }
                    list.Sort();
                }
                result[location] = list;
            }

            return result;
        }

        private static void AddToList(List<HPUsage> list, WeaponCategoryValue wc)
        {
            var item = list.FirstOrDefault(i => i.WeaponCategoryID == wc.ID);
            if (item != null)
                item.Total += 1;
            else
            {
                item = new HPUsage(HardpointController.Instance[wc.ID], 1);
                if (item.hpInfo != null)
                    list.Add(new HPUsage(item, true));
            }
        }
        private static void SubFromList(List<HPUsage> list, WeaponCategoryValue wc, bool remove = false)
        {
            var item = list.FirstOrDefault(i => i.WeaponCategoryID == wc.ID);
            if (item != null)
            {
                item.Total -= 1;
                if (item.Total <= 0 && remove)
                    list.Remove(item);
            }
        }

        public static WeaponCategoryValue GetWeaponCategory(this WeaponDef weapon)
        {
            return weapon.WeaponCategoryValue;
        }

        public static WeaponCategoryValue GetWeaponCategory(this MechComponentDef cdef)
        {
            if (cdef == null)
                return notSet;

            if (cdef is WeaponDef w)
                return w.WeaponCategoryValue;

            if (!categories.TryGetValue(cdef.Description.Id, out var result))
            {
                result = notSet;
                categories[cdef.Description.Id] = result;
            }

            return result;

        }

        public static WeaponCategoryValue GetWeaponCategory(this MechComponentRef cref)
        {
            if (cref == null)
                return notSet;

            if (cref.ComponentDefType == ComponentType.Weapon)
                return (cref.Def as WeaponDef)?.WeaponCategoryValue ?? notSet;

            if (!categories.TryGetValue(cref.ComponentDefID, out var result))
            {
                result = notSet;
                categories[cref.ComponentDefID] = result;
            }

            return result;
        }


    }
}