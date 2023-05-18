#nullable disable
// ReSharper disable InconsistentNaming
using BattleTech;

namespace CustomComponents.InventoryUnlimited.Patches;

// TODO add hooks to Add and Remove, then move this to init or something
[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.GetAllInventoryStrings))]
internal static class SimGameState_GetAllInventoryStrings
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(SimGameState __instance)
    {
        var companyStats = __instance.CompanyStats;
        foreach (var statItemId in UnlimitedFeature.UnlimitedItemStatIds)
        {
            companyStats.RemoveStatistic(statItemId);
            companyStats.RemoveStatistic(statItemId + ".DAMAGED");
        }
    }
}