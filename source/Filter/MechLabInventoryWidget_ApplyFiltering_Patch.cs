using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabInventoryWidget), "ApplyFiltering")]
    internal static class MechLabInventoryWidget_ApplyFiltering_Patch
    {
        internal static void Postfix(MechLabInventoryWidget __instance, float ___mechTonnage,
            List<InventoryItemElement_NotListView> ___localInventory)
        {
            var helper = new MechLabHelper(__instance.ParentDropTarget as MechLabPanel);

            foreach (var item in ___localInventory)
            {
                if (!item.GameObject.activeSelf)
                    continue;

                var filter = MechLabFilter.ApplyFilter(helper, item.ComponentRef.Def);

                if(!filter)
                    item.gameObject.SetActive(false);
            }
        }
    }
}
