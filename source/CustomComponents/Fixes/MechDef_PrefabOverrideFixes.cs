using System.Collections.Generic;
using BattleTech;

namespace CustomComponents.Fixes;

[HarmonyPatch(typeof(MechDef))]
internal static class MechDef_PrefabOverrideFixes
{
    [HarmonyPrepare]
    public static bool Prepare() => Control.Settings.PrefabOverrideFixes;

    private static readonly Dictionary<string, string> PrefabOverridesCache = new();

    [HarmonyPatch(nameof(MechDef.FromJSON))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void MechDef_FromJSON_Postfix(MechDef __instance)
    {
        var mechDef = __instance;

        if (string.IsNullOrEmpty(mechDef.prefabOverride))
        {
            return;
        }

        var chassisId = mechDef.chassisID;
        if (string.IsNullOrEmpty(chassisId))
        {
            Log.PrefabOverrideCache.Warning?.Log($"chassisID missing for MechDef {mechDef.Description.Id}");
            return;
        }

        var expectedMechDefId = chassisId.Replace("chassisdef_", "mechdef_");
        if (mechDef.Description.Id != expectedMechDefId)
        {
            Log.PrefabOverrideCache.Trace?.Log($"Ignoring prefabOverride from MechDef {mechDef.Description.Id} as it does not match ChassisDef {chassisId}");
            return;
        }

        Log.PrefabOverrideCache.Debug?.Log($"Adding prefabOverride {mechDef.prefabOverride} for {mechDef.chassisID}");
        PrefabOverridesCache[mechDef.chassisID] = mechDef.prefabOverride;
    }

    [HarmonyPatch(nameof(MechDef.Chassis), MethodType.Setter)]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void MechDef_set_Chassis_Prefix(MechDef __instance, ChassisDef __0)
    {
        var mechDef = __instance;
        var chassisDef = __0;

        if (!string.IsNullOrEmpty(mechDef.prefabOverride))
        {
            return;
        }

        var chassisId = chassisDef?.Description?.Id ?? __instance.chassisID;
        if (chassisId == null)
        {
            // apparently can happen during json loading if chassis is set to null after prefabOverride was already set
            return;
        }

        if (PrefabOverridesCache.TryGetValue(chassisId, out var prefabOverride))
        {
            Log.PrefabOverrideCache.Trace?.Log($"Found prefabOverride {prefabOverride} for {chassisId}");
            mechDef.prefabOverride = prefabOverride;
        }
    }
}