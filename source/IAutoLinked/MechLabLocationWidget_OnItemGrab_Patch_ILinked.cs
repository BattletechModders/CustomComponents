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
            if (__result && item.ComponentRef.Def is IAutoLinked linked && linked.Links != null)
            {
                var helper = new MechLabHelper(___mechLab);
                foreach (var r_link in linked.Links)
                {
                    var widget = helper.GetLocationWidget(r_link.Location);
                    if (widget != null)
                    {
                        Control.Logger.LogDebug($"{r_link.ApendixID} from {r_link.Location}");
                        var location = new LocationHelper(widget);
                        var remove = location.LocalInventory.FirstOrDefault(e =>
                            e?.ComponentRef?.ComponentDefID == r_link.ApendixID);

                        if (remove != null)
                        {
                            Control.Logger.LogDebug($"removed");
                            widget.OnRemoveItem(remove, true);

                            if (item.ComponentRef.Def is IDefault)
                                GameObject.Destroy(remove.gameObject);
                            else
                            {
                                var temp = ___mechLab.DragItem as MechLabItemSlotElement;
                                ___mechLab.ForceItemDrop(remove);
                                helper.SetDragItem(temp);
                            }
                        }
                        else
                            Control.Logger.LogDebug($"not found");
                    }
                }
            }
        }
    }
}