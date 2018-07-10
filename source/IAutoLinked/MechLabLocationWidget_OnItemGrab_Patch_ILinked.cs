using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    public static class MechLabLocationWidget_OnItemGrab_Patch_Linked
    {

        public static void Postfix(bool __result, MechLabPanel ___mechLab, MechLabLocationWidget __instance,
            IMechLabDraggableItem item)
        {
            if (__result && item.ComponentRef.Is<AutoLinked>(out var linked) && linked.Links != null)
            {
                LinkedController.RemoveLinked(___mechLab, item, linked);
            }
        }

        
    }
}