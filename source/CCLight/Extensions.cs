using BattleTech;
using System.Collections.Generic;

namespace CustomComponents
{
    public static class Extensions
    {
        public static T GetComponent<T>(this MechComponentDef def)
        {
            return Database.GetCustomComponent<T>(def);
        }

        public static IEnumerable<T> GetComponents<T>(this MechComponentDef def)
        {
            return Database.GetCustomComponents<T>(def);
        }

        public static T GetComponent<T>(this MechComponentRef @ref)
        {
            return Database.GetCustomComponent<T>(@ref);
        }

        public static IEnumerable<T> GetComponents<T>(this MechComponentRef @ref)
        {
            return Database.GetCustomComponents<T>(@ref);
        }

        public static bool Is<T>(this MechComponentDef def, out T res)
        {
            res = Database.GetCustomComponent<T>(def);
            return res == null;
        }

        public static bool Is<T>(this MechComponentRef @ref, out T res)
        {
            res = Database.GetCustomComponent<T>(@ref);
            return res == null;
        }
    }
}