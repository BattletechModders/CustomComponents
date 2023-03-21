using System;
using BattleTech.UI;

namespace CustomComponents;

[HarmonyPatch(typeof(MechLabInventoryWidget), nameof(MechLabInventoryWidget.ApplySorting))]
internal static class MechLabInventoryWidget_ApplySorting_Patch
{
    private static Comparison<InventoryItemElement_NotListView> currentSort;

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(ref bool __runOriginal, MechLabInventoryWidget __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.currentSort == null)
        {
            return;
        }

        if (__instance.currentSort == currentSort)
        {
            return;
        }

        currentSort = new InventorySorterNotListComparer(__instance.currentSort).Compare;
        __instance.currentSort = currentSort;
    }
}