using System;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents.Patches
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
            try
            {
                if (Control.Settings.DontUseFilter)
                    return;

                Control.LogDebug(DType.Filter, "StartFilter");
                int empty_item = 0;
                foreach (var item in ___localInventory)
                {
                    if (item == null)
                    {
                        empty_item += 1;
                        continue;

                    }

                    Control.LogDebug(DType.Filter, $"-- item: {item.ItemType}. ref: {(item.ComponentRef == null ? "NULL!" : item.ComponentRef.ComponentDefID)}");


                    //if item already hidden - skip
                    if (!item.GameObject.activeSelf)
                        continue;

                    var mechlab = __instance.ParentDropTarget as MechLabPanel;
                    if (item.ComponentRef != null)
                    {
                        if(item.ComponentRef.HasFlag(CCF.HideFromInv))
                        {
                            item.gameObject.SetActive(false);
                            Control.LogDebug(DType.Filter, $"---- filterd, hide from inventory/default");
                        }
                        foreach (var filter in item.ComponentRef.GetComponents<IMechLabFilter>())
                        {
                            try
                            {
                                Control.LogDebug(DType.Filter, $"--- {filter.GetType()}");
                                if (!filter.CheckFilter(mechlab))
                                {
                                    item.gameObject.SetActive(false);
                                    Control.LogDebug(DType.Filter, $"---- filterd, stoped");
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                Control.LogError("Error in filter", e);
                            }
                        }
                    }
                    else
                        Control.LogDebug(DType.Filter, $"-- ITEM IS NULL!");

                }

                if (empty_item > 0)
                {
                    Control.LogError($"found {empty_item} broken items, trying to clear");
                    ___localInventory.RemoveAll(o => o == null);
                }
                Control.LogDebug(DType.Filter, "EndFilter");

            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}
