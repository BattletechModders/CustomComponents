using System;
using System.Collections.Generic;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "SetData")]
    public static class MechLabLocationWidget_SetData_Patch
    {
        public static void Postfix(List<MechLabItemSlotElement> ___localInventory)
        {
            try
            {
                SortWidgetInventory(___localInventory);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }

        internal static void SortWidgetInventory(List<MechLabItemSlotElement> inventory)
        {
            SorterUtils.SortWidgetInventory(inventory);

            for (var index = 0; index < inventory.Count; index++)
            {
                var element = inventory[index];
                element.gameObject.transform.SetSiblingIndex(index);

                //Control.Logger.LogDebug($"id={element.ComponentRef.Def.Description.Id} index={index} order={Order(element)}");
            }
        }
    }
}