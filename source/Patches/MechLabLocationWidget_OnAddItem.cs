using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnAddItem")]
    public static class MechLabLocationWidget_OnAddItem_Patch
    {
        public static void Postfix(IMechLabDraggableItem item, List<MechLabItemSlotElement> ___localInventory)
        {
            if (item.ComponentRef?.Def == null || !item.ComponentRef.Def.Is<ISorter>())
            {
                return;
            }

            MechLabLocationWidget_SetData_Patch.Sorter.SortWidgetInventory(___localInventory);
        }
    }
}