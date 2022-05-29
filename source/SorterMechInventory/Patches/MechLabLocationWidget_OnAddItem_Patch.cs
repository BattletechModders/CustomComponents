using System;
using System.Collections.Generic;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnAddItem")]
    public static class MechLabLocationWidget_OnAddItem_Patch
    {
        public static void Postfix(IMechLabDraggableItem item, List<MechLabItemSlotElement> ___localInventory)
        {
            try
            {
                if (item.ComponentRef?.Def == null || !item.ComponentRef.Def.Is<ISorter>())
                {
                    return;
                }

                MechLabLocationWidget_SetData_Patch.SortWidgetInventory(___localInventory);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}