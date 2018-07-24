using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using HBS.Logging;

namespace CustomComponents
{
    internal static class Database
    {
        internal static readonly Dictionary<string, List<ICustomComponent>> CustomComponents
            = new Dictionary<string, List<ICustomComponent>>();

        public static DataManager DataManager { get; internal set; }


        public static MechComponentDef RefreshDef(string id, ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Weapon:
                    return DataManager.WeaponDefs.Get(id);
                case ComponentType.AmmunitionBox:
                    return DataManager.AmmoBoxDefs.Get(id);
                case ComponentType.HeatSink:
                    return DataManager.HeatSinkDefs.Get(id);
                case ComponentType.JumpJet:
                    return DataManager.JumpJetDefs.Get(id);
                case ComponentType.Upgrade:
                    return DataManager.UpgradeDefs.Get(id);

                default:
                    return null;
            }
        }

        public static T GetCustomComponent<T>(string key)
        {
            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                return default(T);
            }

            return ccs.OfType<T>().FirstOrDefault();
        }

        

        public static IEnumerable<T> GetCustomComponents<T>(string key)
        {
            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                return Enumerable.Empty<T>();
            }

            return ccs.OfType<T>();
        }


        internal static T GetCustomComponent<T>(MechComponentDef def)
        {
            return GetCustomComponent<T>(Key(def));
        }

        internal static IEnumerable<T> GetCustomComponents<T>(MechComponentDef def)
        {
            return GetCustomComponents<T>(Key(def));
        }

        internal static T GetCustomComponent<T>(MechComponentRef cref)
        {

            //if (cref.Def == null)
            //{
            //    if (cref.DataManager == null)
            //    {
            //        if (game.DataManager != null)
            //        {
            //            cref.DataManager = game.DataManager;
            //            cref.RefreshComponentDef();
            //        }
            //        else
            //        {
            //            Control.Logger.Log("No datamanager found!");
            //        }
            //    }
            //    else
            //        cref.RefreshComponentDef();
            //}

            return GetCustomComponent<T>(Key(cref));

        }

        internal static IEnumerable<T> GetCustomComponents<T>(MechComponentRef cref)
        {
            //if (@ref.Def == null)
            //    @ref.RefreshComponentDef();

            return GetCustomComponents<T>(Key(cref));
        }

        internal static void SetCustomComponent(MechComponentDef def, ICustomComponent cc)
        {
            var key = Key(def);

            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                ccs = new List<ICustomComponent>();
                CustomComponents[key] = ccs;
            }

            var old = ccs.FirstOrDefault(i => i.GetType() == cc.GetType());
            if (old != null)
                ccs.Remove(old);

            ccs.Add(cc);
        }

        private static string Key(MechComponentDef def)
        {
            return def.Description.Id;
        }

        private static string Key(MechComponentRef @ref)
        {
            return @ref.ComponentDefID;
        }


        public static bool AlreadyLoaded(MechComponentDef componentDef)
        {
            if (componentDef == null)
                return false;

            return CustomComponents.TryGetValue(Key(componentDef), out _);
        }
    }
}