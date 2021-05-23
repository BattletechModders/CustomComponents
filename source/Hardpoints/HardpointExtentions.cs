using System.Collections.Generic;
using System.Linq;
using BattleTech;
using FluffyUnderware.DevTools.Extensions;

namespace CustomComponents
{
    public static class HardpointExtentions
    {
        public class WeaponDefaultRecord
        {
            public MechComponentDef Def { get; set; }
            public WeaponCategoryValue WeaponCategory { get; set; }
            public HashSet<string> Categories { get; set; }
            public bool HaveCategories => Categories != null && Categories.Count > 0;

            public ChassisLocations Location { get; set; }

        }

        private static Dictionary<string, ChassisLocations> have_defaults = new Dictionary<string,ChassisLocations>();
        private static Dictionary<string, WeaponCategoryValue> categories = new Dictionary<string, WeaponCategoryValue>();
        private static Dictionary<string, List<WeaponDefaultRecord>> defaults = new Dictionary<string, List<WeaponDefaultRecord>>();

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