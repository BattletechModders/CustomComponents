using BattleTech;
using System;
using System.Collections.Generic;

namespace CustomComponents
{
    public static class MechComponentDefExtensions
    {
        public static T GetComponent<T>(this MechComponentDef target)
        {
            return Database.GetCustom<T>(target);
        }

        public static IEnumerable<T> GetComponents<T>(this MechComponentDef target)
        {
            return Database.GetCustoms<T>(target);
        }

        public static bool Is<T>(this MechComponentDef target, out T res)
        {
            return Database.Is(target, out res);
        }

        public static bool Is<T>(this MechComponentDef target)
        {
            return Database.Is<T>(target);
        }

        public static T AddComponent<T>(this MechComponentDef target, T component) where T : ICustom
        {
            if (component is SimpleCustom<MechComponentDef> simple)
            {
                simple.Def = target;
            }
            Database.AddCustom(target, component);
            return component;
        }

        public static T GetOrCreate<T>(this MechComponentDef target, Func<T> factory) where T : ICustom
        {
            var result = target.GetComponent<T>();
            if ((result is ExtendedDetails.ExtendedDetails ed) && ed.Def != target.Description)
            {
                ed.Def = target.Description;
            }
            return result ?? target.AddComponent(factory.Invoke());
        }
    }

    public static class VehicleExtentions
    {
        public static T GetComponent<T>(this VehicleChassisDef target)
        {
            return Database.GetCustom<T>(target);
        }

        public static IEnumerable<T> GetComponents<T>(this VehicleChassisDef target)
        {
            return Database.GetCustoms<T>(target);
        }

        public static bool Is<T>(this VehicleChassisDef target, out T res)
        {
            return Database.Is(target, out res);
        }

        public static bool Is<T>(this VehicleChassisDef target)
        {
            return Database.Is<T>(target);
        }
    }

    public static class MechDefExtensions
    {
        public static T GetComponent<T>(this MechDef target)
        {
            return Database.GetCustom<T>(target);
        }

        public static IEnumerable<T> GetComponents<T>(this MechDef target)
        {
            return Database.GetCustoms<T>(target);
        }

        public static bool Is<T>(this MechDef target, out T res)
        {
            return Database.Is(target, out res);
        }

        public static bool Is<T>(this MechDef target)
        {
            return Database.Is<T>(target);
        }

        public static bool IsBroken(this MechDef def)
        {
            try
            {
                if (def == null)
                {
                    Control.LogError("MECHDEF IS NULL!");
                    return true;
                }
                if (def.Chassis == null)
                {
                    Control.LogError($"Chassis of {def.Description.Id} IS NULL!");
                    return true;
                }
                if (def.MechTags == null)
                {
                    Control.LogError($"Mechtags of {def.Description.Id} IS NULL!");
                    return true;
                }
                if (def.Chassis.ChassisTags == null)
                {
                    Control.LogError($"Chassistags of {def.Description.Id} IS NULL!");
                    return true;
                }

                return false;
            }
            catch
            {
                Control.LogError("5.GOT NRE!!!!");
                Control.LogError($"{def}");
                return false;
            }
        }
    }

    public static class ChassisDefExtensions
    {
        public static T GetComponent<T>(this ChassisDef target)
        {
            return Database.GetCustom<T>(target);
        }

        public static IEnumerable<T> GetComponents<T>(this ChassisDef target)
        {
            return Database.GetCustoms<T>(target);
        }

        public static bool Is<T>(this ChassisDef target, out T res)
        {
            return Database.Is(target, out res);
        }

        public static bool Is<T>(this ChassisDef target)
        {
            return Database.Is<T>(target);
        }

        public static T AddComponent<T>(this ChassisDef target, T component) where T : ICustom
        {
            if (component is SimpleCustom<ChassisDef> simple)
            {
                simple.Def = target;
            }
            Database.AddCustom(target, component);
            return component;
        }

        public static T GetOrCreate<T>(this ChassisDef target, Func<T> factory) where T : ICustom
        {
            var result = target.GetComponent<T>();
            if ((result is ExtendedDetails.ExtendedDetails ed) && ed.Def != target.Description)
            {
                ed.Def = target.Description;
            }

            return result ?? target.AddComponent(factory.Invoke());
        }
    }

    public static class MechComponentRefExtensions
    {
        public static T GetComponent<T>(this BaseComponentRef target)
        {
            RefreshDef(target);
            return target.Def.GetComponent<T>();
        }

        public static IEnumerable<T> GetComponents<T>(this BaseComponentRef target)
        {
            RefreshDef(target);
            return target.Def.GetComponents<T>();
        }

        public static bool Is<T>(this BaseComponentRef target, out T res)
        {
            RefreshDef(target);
            return target.Def.Is(out res);
        }

        public static bool Is<T>(this BaseComponentRef target)
        {
            RefreshDef(target);
            return target.Def.Is<T>();
        }

        internal static void RefreshDef(this BaseComponentRef target)
        {
            if (target == null)
            {
                return;
            }
            if (target.Def != null)
            {
                return;
            }
            target.DataManager ??= UnityGameInstance.BattleTechGame.DataManager;
            target.RefreshComponentDef();
        }

        public static T GetComponent<T>(this MechComponentRef target)
        {
            RefreshDef(target);
            return target.Def.GetComponent<T>();
        }

        public static IEnumerable<T> GetComponents<T>(this MechComponentRef target)
        {
            RefreshDef(target);
            return target.Def.GetComponents<T>();
        }

        public static bool Is<T>(this MechComponentRef target, out T res)
        {
            RefreshDef(target);
            return target.Def.Is(out res);
        }

        public static bool Is<T>(this MechComponentRef target)
        {
            RefreshDef(target);
            return target.Def.Is<T>();
        }
    }
}