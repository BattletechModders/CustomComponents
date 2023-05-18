#nullable disable
// ReSharper disable InconsistentNaming
using BattleTech.UI;

namespace CustomComponents.InventoryUnlimited.Patches;

[HarmonyPatch(typeof(ListElementController_BASE_NotListView), nameof(ListElementController_BASE_NotListView.ModifyQuantity))]
internal static class ListElementController_BASE_NotListView_ModifyQuantity
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(ref bool __runOriginal, ListElementController_BASE_NotListView __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.quantity == UnlimitedFeature.UnlimitedCount)
        {
            __runOriginal = false;
        }
    }
}