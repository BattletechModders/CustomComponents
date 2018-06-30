using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabInventoryWidget), "ApplyFiltering")]
    internal static class MechLabInventoryWidget_ApplyFiltering
    {
        internal static void Postfix(MechLabInventoryWidget __instance, float ___mechTonnage,
            List<InventoryItemElement_NotListView> ___localInventory)
        {
            foreach (var item in ___localInventory)
            {
                MechComponentDef component = null;

                if (item.controller != null)
                    component = item.controller.componentDef;
                else if (item.ComponentRef != null)
                    component = item.ComponentRef.Def;

                if (component != null && (component is IWeightLimited))
                {
                    var limit = component as IWeightLimited;

                    item.gameObject.SetActive(
                        (___mechTonnage < 0 ||
                        ___mechTonnage >= limit.MinTonnage && ___mechTonnage <= limit.MaxTonnage) && item.gameObject.activeSelf
                        );
                }
            }
        }
    }
}
