#nullable disable
// ReSharper disable InconsistentNaming
using System.Collections.Generic;
using BattleTech;

namespace CustomComponents.InventoryUnlimited.Patches;

[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.GetAllInventoryItemDefs))]
internal static class SimGameState_GetAllInventoryItemDefs
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    internal static void Postfix(SimGameState __instance, List<MechComponentRef> __result)
    {
        foreach (var componentDef in UnlimitedFeature.UnlimitedItemDefs)
        {
            var mechComponentRef = new MechComponentRef(componentDef.Description.Id, __instance.GenerateSimGameUID(), componentDef.ComponentType, ChassisLocations.None);
            // mechComponentRef.DataManager = __instance.DataManager;
            mechComponentRef.SetComponentDef(componentDef);
            // mechComponentRef.RefreshComponentDef();
            __result.Add(mechComponentRef);
        }
    }
}