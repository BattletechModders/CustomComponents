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

        private static GameInstance game;

        internal static T GetCustomComponent<T>(MechComponentDef def)
        {
            var key = Key(def);

            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                return default(T);
            }

            return ccs.OfType<T>().FirstOrDefault();
        }

        internal static IEnumerable<T> GetCustomComponents<T>(MechComponentDef def)
        {
            var key = Key(def);

            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                return Enumerable.Empty<T>();
            }

            return ccs.OfType<T>();
        }

        internal static T GetCustomComponent<T>(MechComponentRef cref)
        {

            if (cref.Def == null)
            {
                if (cref.DataManager == null)
                {
                    if (game.DataManager != null)
                    {
                        cref.DataManager = game.DataManager;
                        cref.RefreshComponentDef();
                    }
                    else
                    {
                        Control.Logger.Log("No datamanager found!");
                    }
                }
                else
                    cref.RefreshComponentDef();
            }

            var key = Key(cref);

            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                return default(T);
            }

            return ccs.OfType<T>().FirstOrDefault();
        }

        internal static IEnumerable<T> GetCustomComponents<T>(MechComponentRef @ref)
        {
            if (@ref.Def == null)
                @ref.RefreshComponentDef();

            var key = Key(@ref);

            if (!CustomComponents.TryGetValue(key, out var ccs))
            {
                return Enumerable.Empty<T>();
            }

            return ccs.OfType<T>();
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

        internal static void setDataManager(GameInstance value)
        {
            game = value;
        }
    }
}