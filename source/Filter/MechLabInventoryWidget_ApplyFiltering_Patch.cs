using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents
{
    /// <summary>
    /// pathing filtering
    /// </summary>
    [HarmonyPatch(typeof(MechLabInventoryWidget), "ApplyFiltering")]
    internal static class MechLabInventoryWidget_ApplyFiltering_Patch
    {

        internal static void Postfix(MechLabInventoryWidget __instance, float ___mechTonnage,
            List<InventoryItemElement_NotListView> ___localInventory)
        {
            //if cc prebuilded validators not enabled - left all as is
            if (!Control.settings.LoadDefaultValidators)
                return;

            foreach (var item in ___localInventory)
            {
                //if item already hidden - skip
                if (!item.GameObject.activeSelf)
                    continue;

                var mechlab = __instance.ParentDropTarget as MechLabPanel;

                foreach (var filter in item.ComponentRef.GetComponents<IMechLabFilter>())
                {
                    if(!filter.CheckFilter(mechlab))
                    {
                        item.gameObject.SetActive(false);
                        return;
                    }
                }

            }
        }
    }
}
