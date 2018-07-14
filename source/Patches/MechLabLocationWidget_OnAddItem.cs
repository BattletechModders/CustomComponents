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
        public static void Postfix(IMechLabDraggableItem item, MechLabLocationWidget __instance, List<MechLabItemSlotElement> ___localInventory, Transform ___inventoryParent)
        {
            if(item.ComponentRef.Def == null || !item.ComponentRef.Def.Is<Sorter>(out var sorter))
                return;
            
            if(sorter.Order < 0 || sorter.Order >= ___localInventory.Count)
                return;

            var element = item as MechLabItemSlotElement;

            ___localInventory.Remove(element);
            ___localInventory.Insert(sorter.Order, element);
            item.GameObject.transform.SetSiblingIndex(sorter.Order);

        }
    }
}