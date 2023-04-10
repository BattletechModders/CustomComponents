using System;
using System.Collections.Generic;
using BattleTech.UI;

namespace CustomComponents.Patches;

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
            {
                return;
            }

            Log.Filter.Trace?.Log("StartFilter");
            var empty_item = 0;
            foreach (var item in ___localInventory)
            {
                if (item == null)
                {
                    empty_item += 1;
                    continue;

                }

                Log.Filter.Trace?.Log($"-- item: {item.ItemType}. ref: {(item.ComponentRef == null ? "NULL!" : item.ComponentRef.ComponentDefID)}");


                //if item already hidden - skip
                if (!item.GameObject.activeSelf)
                {
                    continue;
                }

                var mechlab = __instance.ParentDropTarget as MechLabPanel;
                if (item.ComponentRef != null)
                {
                    if(item.ComponentRef.Def.CCFlags().HideFromInv)
                    {
                        item.gameObject.SetActive(false);
                        Log.Filter.Trace?.Log("---- filterd, hide from inventory/default");
                    }
                    foreach (var filter in item.ComponentRef.GetComponents<IMechLabFilter>())
                    {
                        try
                        {
                            Log.Filter.Trace?.Log($"--- {filter.GetType()}");
                            if (!filter.CheckFilter(mechlab))
                            {
                                item.gameObject.SetActive(false);
                                Log.Filter.Trace?.Log("---- filterd, stoped");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Main.Error?.Log("Error in filter", e);
                        }
                    }
                }
                else
                {
                    Log.Filter.Trace?.Log("-- ITEM IS NULL!");
                }
            }

            if (empty_item > 0)
            {
                Log.Main.Error?.Log($"found {empty_item} broken items, trying to clear");
                ___localInventory.RemoveAll(o => o == null);
            }
            Log.Filter.Trace?.Log("EndFilter");

        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}