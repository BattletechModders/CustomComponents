using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents;

public class Database
{
    #region internal

    internal static bool SetCustomWithIdentifier(string identifier, ICustom cc)
    {
        return Shared.SetCustomInternal(identifier, cc);
    }

    public static IEnumerable<T> GetCustoms<T>(object target)
    {
        var identifier = Identifier(target);
        return Shared.GetCustomsInternal<T>(identifier);
    }

    public static T GetCustom<T>(object target)
    {
        var identifier = Identifier(target);
        return Shared.GetCustomInternalFast<T>(identifier);
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

    internal static void AddCustom(object target, ICustom cc)
    {
        var identifier = Identifier(target);
        var ccs = Shared.GetOrCreateCustomsList(identifier);
        ccs.Add(cc);
    }
        
    internal static string Identifier(object target)
    {
        if (target == null)
        {
            Log.Main.Error?.Log("Error - requested identifier for null!");
            return string.Empty;
        }

        // this is for faster access
        if (target is MechComponentDef mechComponentDef)
        {
            return mechComponentDef.Description.Id;
        }
        else if (target is ChassisDef chassisDef)
        {
            return chassisDef.Description.Id;
        }
        else if (target is VehicleChassisDef vcd)
            return vcd.Description.Id;

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
        if (key == null || !customs.TryGetValue(key, out var ccs))
        {
            return Enumerable.Empty<T>();
        }

        return ccs.OfType<T>();
    }

    private T GetCustomInternalFast<T>(string key)
    {
        if (key != null && customs.TryGetValue(key, out var ccs))
        {
            for (var index = 0; index < ccs.Count; index++)
            {
                if (ccs[index] is T csT)
                {
                    return csT;
                }
            }
        }
        return default;
    }

    private List<ICustom> GetOrCreateCustomsList(string key)
    {
        if (!customs.TryGetValue(key, out var ccs))
        {
            ccs = new List<ICustom>();
            customs[key] = ccs;
        }
        return ccs;
    }

    private bool SetCustomInternal(string key, ICustom cc)
    {
        Log.CCLoading.Trace?.Log($"SetCustomInternal key={key} cc={cc}");

        var ccs = GetOrCreateCustomsList(key);

        var attribute = Registry.GetAttributeByType(cc.GetType());

        if (attribute != null)
            for (var i = 0; i < ccs.Count; i++)
            {
                var custom = ccs[i];
                var attribute2 = Registry.GetAttributeByType(custom.GetType());
                    
                if (attribute2 == null)
                    continue;

                var same_type = string.IsNullOrEmpty(attribute.Group)
                    ? attribute.Name == attribute2.Name
                    : attribute.Group == attribute2.Group;

                if (!same_type)
                    continue;

                if (attribute.AllowArray)
                {
                    if (!(cc is IReplaceIdentifier cci1))
                        break;

                    if (custom is IReplaceIdentifier cci2 &&
                        cci1.ReplaceID == cci2.ReplaceID)
                    {
                        Log.CCLoading.Trace?.Log($"--find replace: add:{attribute.Name} fnd:{attribute2.Name} grp:{attribute.Group} rid:{cci1.ReplaceID}");
                        Log.CCLoading.Trace?.Log($"--replace: from:{custom} to:{cc}");
                        ccs[i] = cc;
                        return true;
                    }
                }
                else
                {
                    Log.CCLoading.Trace?.Log($"--find replace: add:{attribute.Name} fnd:{attribute2.Name} grp:{attribute.Group}");
                    Log.CCLoading.Trace?.Log($"--replace: from:{custom} to:{cc}");
                    ccs[i] = cc;
                    return true;
                }
            }

        Log.CCLoading.Trace?.Log($"--added");
        ccs.Add(cc);
        return true;
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
            try
            {
                if (defs)
                {
                    Shared.Clear();
                }
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log(e);
            }
        }
    }

    #endregion
}