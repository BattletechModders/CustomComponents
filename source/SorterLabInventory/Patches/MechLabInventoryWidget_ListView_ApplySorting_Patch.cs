using BattleTech.UI;

namespace CustomComponents;

[HarmonyPatch(typeof(MechLabInventoryWidget_ListView), nameof(MechLabInventoryWidget_ListView.ApplySorting))]
internal static class MechLabInventoryWidget_ListView_ApplySorting_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(ref bool __runOriginal, MechLabInventoryWidget_ListView __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.invertSort)
        {
            return;
        }

        if (__instance.currentListItemSorter is InventorySorterListComparer)
        {
            return;
        }

        __instance.currentListItemSorter = new InventorySorterListComparer(__instance.currentListItemSorter.Compare);
        __instance.currentSort = new InventorySorterListComparer(__instance.currentSort).Compare;
        __instance.invertSort = false;
    }
}