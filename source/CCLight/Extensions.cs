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
            Database.AddCustom(target, component);
            return component;
        }

        public static T GetOrCreate<T>(this MechComponentDef target, Func<T> factory) where T : ICustom
        {
            return target.GetComponent<T>() ?? target.AddComponent(factory.Invoke());
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
            return def.MechTags.IgnoreAutofix() || def.Chassis.IgnoreAutofix();
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
            Database.AddCustom(target, component);
            return component;
        }

        public static T GetOrCreate<T>(this ChassisDef target, Func<T> factory) where T : ICustom
        {
            return target.GetComponent<T>() ?? target.AddComponent(factory.Invoke());
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
            return set.Contains(Control.Settings.IgnoreAutofixTag);
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