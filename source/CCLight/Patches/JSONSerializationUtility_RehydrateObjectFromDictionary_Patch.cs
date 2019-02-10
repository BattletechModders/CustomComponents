using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;
using Harmony;
using HBS.Extensions;
using HBS.Util;

namespace CustomComponents
{
    [HarmonyPatch]
    public static class JSONSerializationUtility_RehydrateObjectFromDictionary_Patch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(JSONSerializationUtility).GetMethod("RehydrateObjectFromDictionary",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static void Prefix(object target, Dictionary<string, object> values)
        {
            if (!Control.Settings.TestEnableAllTags)
            {
                return;
            }

            var baseTags = new[] {"ComponentTags", "MechTags"};
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

        public static void Postfix(object target, Dictionary<string, object> values)
        {
            Registry.ProcessCustomFactories(target, values);
            if (target == null)
            {
                Control.LogError($"NULL item loaded");
                foreach (var value in values)
                {
                    Control.LogError($"- {value.Key}: {value.Value}");
                }
            }

            if (target is MechComponentDef def)
            {
                var description = def.GetComponents<IAdjustDescription>()
                    .Aggregate("", (current, adjuster) => adjuster.AdjustDescription(current));

                if (description != "")
                {
                    var trav = new Traverse(def.Description).Property<string>("Details");
                    trav.Value = def.Description.Details + "\n" + description;
                }
            }
        }
    }
}
