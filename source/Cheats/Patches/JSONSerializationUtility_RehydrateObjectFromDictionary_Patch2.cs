using System;
using System.Collections.Generic;
using System.Reflection;
using HBS.Util;

namespace CustomComponents;

[HarmonyPatch]
public static class JSONSerializationUtility_RehydrateObjectFromDictionary_Patch2
{
    public static bool Prepare()
    {
        return Control.Settings.DEBUG_EnableAllTags;
    }

    public static MethodBase TargetMethod()
    {
        return typeof(JSONSerializationUtility)
            .GetMethod("RehydrateObjectFromDictionary", BindingFlags.NonPublic | BindingFlags.Static);
    }

    public static void Prefix(object target, Dictionary<string, object> values)
    {
        try
        {
            var baseTags = new[] { "ComponentTags", "MechTags" };
            foreach (var baseTag in baseTags)
                if (values.TryGetValue(baseTag, out var Tags))
                {
                    if (!(Tags is Dictionary<string, object> tags))
                    {
                        continue;
                    }

                    if (tags.TryGetValue("items", out var Items))
                    {
                        if (!(Items is List<object> items))
                        {
                            continue;
                        }

                        items.Remove("BLACKLISTED");
                        items.Remove("component_type_debug");
                        items.Remove("component_type_lostech");
                        items.Add("component_type_stock");

                        //items.Remove("unit_custom");
                        items.Add("unit_release");
                    }
                }

        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}