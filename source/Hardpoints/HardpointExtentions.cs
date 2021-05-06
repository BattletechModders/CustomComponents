using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public static class HardpointExtentions
    {
        private static Dictionary<string, bool> have_defaults = new Dictionary<string, bool>();
        private static Dictionary<string, WeaponCategoryValue> categories = new Dictionary<string, WeaponCategoryValue>();
        private static WeaponCategoryValue notSet = WeaponCategoryEnumeration.GetNotSetValue();
        public static bool HasWeaponDefaults(this MechDef mech)
        {
            if (mech == null)
                return false;
            if (!have_defaults.TryGetValue(mech.ChassisID, out bool result))
            {
                result = mech.Chassis.Is<WeaponDefault>();
                have_defaults[mech.ChassisID] = result;
            }
            return result;
        }

        public static bool HasWeaponDefaults(this ChassisDef chassis)
        {
            if (chassis == null)
                return false;
            if (!have_defaults.TryGetValue(chassis.Description.Id, out bool result))
            {
                result = chassis.Is<WeaponDefault>();
                have_defaults[chassis.Description.Id] = result;
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
                return (cref.Def as WeaponDef).WeaponCategoryValue;

            if (!categories.TryGetValue(cref.ComponentDefID, out var result))
            {
                result = notSet;
                categories[cref.ComponentDefID] = result;
            }

            return result;
        }


    }
}