#nullable disable
// ReSharper disable InconsistentNaming
using BattleTech;

namespace CustomComponents.InventoryUnlimited.Patches;

[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.GetItemCount), typeof(string), typeof(SimGameState.ItemCountType))]
internal static class SimGameState_GetItemCount
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(ref bool __runOriginal, ref int __result, string id)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (UnlimitedFeature.IsUnlimitedByStatItemId(id))
        {
            __result = UnlimitedFeature.UnlimitedCount;
            __runOriginal = false;
        }
    }
}