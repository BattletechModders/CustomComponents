#nullable disable
// ReSharper disable InconsistentNaming
using BattleTech.UI;

namespace CustomComponents.InventoryUnlimited.Patches;

[HarmonyPatch]
internal static class ListElementController_BASE_NotListView_RefreshQuantity
{
    [HarmonyPatch(typeof(ListElementController_BASE_NotListView), nameof(ListElementController_BASE_NotListView.RefreshQuantity))]
    [HarmonyPatch(typeof(ListElementController_InventoryGear_NotListView), nameof(ListElementController_InventoryGear_NotListView.RefreshQuantity))]
    [HarmonyPatch(typeof(ListElementController_InventoryWeapon_NotListView), nameof(ListElementController_InventoryWeapon_NotListView.RefreshQuantity))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(ref bool __runOriginal, ListElementController_BASE_NotListView __instance, InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.quantity == UnlimitedFeature.UnlimitedCount)
        {
            theWidget.qtyElement.SetActive(false);
            __runOriginal = false;
        }
    }
}