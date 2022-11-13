using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabInventoryWidget_ListView), nameof(MechLabInventoryWidget_ListView.ApplySorting))]
    internal static class MechLabInventoryWidget_ListView_ApplySorting_Patch
    {
        internal static void Prefix(MechLabInventoryWidget_ListView __instance)
        {
            try
            {
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
            catch (Exception e)
            {
                Logging.Error?.Log(e);
            }
        }
    }
}