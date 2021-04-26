using BattleTech;
using HBS.Collections;
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

        public static bool IgnoreAutofix(this MechDef def)
        {
            //Control.LogError("start check");
            try
            {

                //Control.LogError("1.mech");
                if (def == null)
                {
                    Control.LogError("MECHDEF IS NULL!");
                    return true;
                }
                //Control.LogError("2.chassis");
                if (def.Chassis == null)
                {
                    Control.LogError($"Chassis of {def.Description.Id} IS NULL!");
                    return true;
                }
                //Control.LogError("3.mech tags");
                if (def.MechTags == null)
                {
                    Control.LogError($"Mechtags of {def.Description.Id} IS NULL!");
                    return true;
                }
                //Control.LogError("4.chassis tags");
                if (def.Chassis.ChassisTags == null)
                {
                    Control.LogError($"Chassistags of {def.Description.Id} IS NULL!");
                    return true;
                }

                try
                {
                    return def.MechTags.IgnoreAutofix() || def.Chassis.IgnoreAutofix();
                }
                catch
                {
                    //Control.LogError("5.got error");
                    //Control.LogError($"got error. try to show {def}");
                    //Control.LogError($"Tags of {def.ChassisID} not null but null. WTF???  IS NULL!");
                    return true;
                }

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

        public static bool IgnoreAutofix(this ChassisDef def)
        {
            return def.ChassisTags.IgnoreAutofix();
        }
    }

    public static class TagSetExtensions
    {
        public static bool IgnoreAutofix(this TagSet set)
        {
            if (set == null)
            {
                Control.LogError("Found empty tagset! disabling autofixer");
                throw new NullReferenceException();
            }

            if (Control.Settings.ignoreAutofixTags == null)
                return false;

            return set.ContainsAny(Control.Settings.ignoreAutofixTags);
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

        private static void RefreshDef(BaseComponentRef target)
        {
            if (target.Def == null)
            {
                if (target.DataManager == null)
                {
                    target.DataManager = UnityGameInstance.BattleTechGame.DataManager;
                }
                target.RefreshComponentDef();
            }
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