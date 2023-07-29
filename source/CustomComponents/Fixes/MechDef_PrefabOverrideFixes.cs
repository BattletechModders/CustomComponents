using System.Collections.Generic;
using BattleTech;

namespace CustomComponents.Fixes;

[HarmonyPatch(typeof(MechDef))]
internal static class MechDef_PrefabOverrideFixes
{
    private static readonly Dictionary<string, string> PrefabOverridesCache = new();

    [HarmonyPatch(nameof(MechDef.FromJSON))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void MechDef_FromJSON_Postfix(MechDef __instance)
    {
        var mechDef = __instance;
        if (string.IsNullOrEmpty(mechDef.chassisID))
        {
            Log.PrefabOverrideCache.Warning?.Log($"chassisID missing for MechDef {mechDef.Description.Id}");
            return;
        }

        if (string.IsNullOrEmpty(mechDef.prefabOverride))
        {
            if (PrefabOverridesCache.Remove(mechDef.chassisID))
            {
                Log.PrefabOverrideCache.Debug?.Log($"Removed chassisID {mechDef.chassisID}");
            }
            return;
        }

        Log.PrefabOverrideCache.Debug?.Log($"Added prefabOverride {mechDef.prefabOverride} for chassisID {mechDef.chassisID}");
        PrefabOverridesCache[mechDef.chassisID] = mechDef.prefabOverride;
    }

    [HarmonyPatch(nameof(MechDef.Chassis), MethodType.Setter)]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void MechDef_set_Chassis_Prefix(MechDef __instance, ChassisDef __0)
    {
        var mechDef = __instance;
        if (!string.IsNullOrEmpty(mechDef.prefabOverride))
        {
            return;
        }

        var chassisId = __0?.Description?.Id ?? __instance.chassisID;
        if (chassisId == null)
        {
            // apparently can happen during json loading if chassis is set to null after prefabOverride was already set
            // Log.PrefabOverrideCache.Warning?.Log($"Chassis and ChassisId missing for MechDef {mechDef.Description?.Id}");
            return;
        }

        if (PrefabOverridesCache.TryGetValue(chassisId, out var prefabOverride))
        {
            Log.PrefabOverrideCache.Trace?.Log($"Found prefabOverride {prefabOverride} for ChassisId {chassisId}");
            mechDef.prefabOverride = prefabOverride;
        }
    }
}