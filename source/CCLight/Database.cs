using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    internal class Database
    {
        #region internal

        internal static void Set(string identifier, ICustom cc)
        {
            Shared.SetCustomInternal(identifier, cc);
        }

        internal static IEnumerable<T> GetCustoms<T>(object target)
        {
            var identifier = Identifier(target);
            if (identifier == null)
            {
                // TODO add logging or throw exception
                return Enumerable.Empty<T>();
            }
            return Shared.GetCustomsInternal<T>(identifier);
        }

        internal static T GetCustom<T>(object target)
        {
            return GetCustoms<T>(target).FirstOrDefault();
        }

        internal static bool Is<T>(object target, out T value)
        {
            value = GetCustom<T>(target);
            return value != null;
        }

        internal static bool Is<T>(object target)
        {
            return GetCustom<T>(target) != null;
        }

        internal static string Identifier(object target)
        {
            // this is for faster access
            if (target is MechComponentDef mechComponentDef)
            {
                return mechComponentDef.Description.Id;
            }

            var descriptionProperty = target.GetType().GetProperty(nameof(MechComponentDef.Description), typeof(DescriptionDef));
            var description = descriptionProperty?.GetValue(target, null) as DescriptionDef;
            return description?.Id;
        }

        #endregion

        #region private

        private static readonly Database Shared = new Database();

        private readonly Dictionary<string, List<ICustom>> customs = new Dictionary<string, List<ICustom>>();

        private IEnumerable<T> GetCustomsInternal<T>(string key)
        {
            if (!customs.TryGetValue(key, out var ccs))
            {
                return Enumerable.Empty<T>();
            }

            return ccs.OfType<T>();
        }
        
        private void SetCustomInternal(string key, ICustom cc)
        {
            //Control.Logger.LogDebug($"SetCustomInternal key={key}");

            if (!customs.TryGetValue(key, out var ccs))
            {
                ccs = new List<ICustom>();
                customs[key] = ccs;
            }

            ccs.RemoveAll(i => i.GetType() == cc.GetType());
            ccs.Add(cc);
        }

        private void Clear()
        {
            customs.Clear();
        }

        #endregion

        #region embedded

        [HarmonyPatch(typeof(DataManager), nameof(DataManager.Clear))]
        public static class DataManager_Clear_Patch
        {
            public static void Prefix(bool defs)
            {
                if (defs)
                {
                    Shared.Clear();
                }
            }
        }

        #endregion
    }
}