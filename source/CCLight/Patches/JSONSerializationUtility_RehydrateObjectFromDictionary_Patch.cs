using System.Collections.Generic;
using System.Reflection;
using Harmony;
using HBS.Util;

namespace CustomComponents
{
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
            Registry.ProcessCustomFactories(target, values);
        }
    }
}
