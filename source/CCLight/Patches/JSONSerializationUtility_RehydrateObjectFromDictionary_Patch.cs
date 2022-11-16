using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using HBS.Util;

namespace CustomComponents;

[HarmonyPatch]
public static class JSONSerializationUtility_RehydrateObjectFromDictionary_Patch
{
    public static MethodBase TargetMethod()
    {
        return typeof(JSONSerializationUtility)
            .GetMethod("RehydrateObjectFromDictionary", BindingFlags.NonPublic | BindingFlags.Static);
    }

    public static void Postfix(object target, Dictionary<string, object> values)
    {
        if (!Control.Loaded)
            return;

        try
        {
            Registry.ProcessCustomFactories(target, values);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log($"Error loading item", e);
        }
    }


}