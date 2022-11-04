using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabInventoryWidget), nameof(MechLabInventoryWidget.ApplySorting))]
    internal static class MechLabInventoryWidget_ApplySorting_Patch
    {
        private static Comparison<InventoryItemElement_NotListView> currentSort;
        internal static void Prefix(MechLabInventoryWidget __instance)
        {
            try
            {
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
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}