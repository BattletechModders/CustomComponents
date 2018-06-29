using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents.WeightLimitation
{
    [HarmonyPatch(typeof(MechLabInventoryWidget), "ApplyFiltering")]
    public static class MechLabInventoryWidget_ApplyFiltering
    {
        public static void Postfix(MechLabInventoryWidget __instance, float ___mechTonnage,
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
                    var tonnage = (component as IWeightLimited).AllowedTonnage;

                    item.gameObject.SetActive(
                        (___mechTonnage < 0 ||
                        ___mechTonnage == tonnage) && item.gameObject.activeSelf
                        );
                }
            }
        }
    }
}
