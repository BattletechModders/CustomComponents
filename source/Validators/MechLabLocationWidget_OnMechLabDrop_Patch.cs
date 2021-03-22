﻿using System;
using System.Linq;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS.Extensions;
using Localize;
using UnityEngine.EventSystems;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnMechLabDrop")]
    internal static class MechLabLocationWidget_OnMechLabDrop_Patch
    {
        [HarmonyPriority(Priority.Low)]
        [HarmonyPrefix]
        public static bool Prefix(MechLabLocationWidget __instance,
            MechLabPanel ___mechLab,
            PointerEventData eventData)
        {
            try
            {
                var dragItem = ___mechLab.DragItem as MechLabItemSlotElement;


                if (!___mechLab.Initialized)
                {
                    return false;
                }

                var newComponentDef = dragItem?.ComponentRef?.Def;
                if (newComponentDef == null)
                {
                    return false;
                }

                Control.LogDebug(DType.ComponentInstall, $"OnMechLabDrop: Adding {newComponentDef.Description.Id}");

                var location_helper = new LocationHelper(__instance);
                var mechlab_helper = new MechLabHelper(___mechLab);

                bool do_cancel(string error, List<IChange> allchanges)
                {
                    if (string.IsNullOrEmpty(error))
                        return false;

                    Control.LogDebug(DType.ComponentInstall, $"- Canceled: {error}");

                    if (allchanges != null)
                    {
                        foreach (IApplyChange change in allchanges)
                        {
                            change.CancelChange(mechlab_helper, location_helper);
                        }
                    }

                    ___mechLab.OnDrop(eventData);
                    ___mechLab.ShowDropErrorMessage(new Text(error));
                    return true;
                }

                Control.LogDebug(DType.ComponentInstall, $"- pre validation");

                foreach (var pre_validator in Validator.GetPre(newComponentDef))
                {

                    if (do_cancel(pre_validator(dragItem, location_helper, mechlab_helper), null))
                        return false;
                }

                Control.LogDebug(DType.ComponentInstall, $"- replace validation");

                var changes = new List<IChange>();
                changes.Add(new AddFromInventoryChange(__instance.loadout.Location, dragItem));

                foreach (ReplaceValidateDropDelegate rep_validator in Validator.GetReplace(newComponentDef))
                    if (do_cancel(rep_validator(dragItem, location_helper, changes), changes))
                        return false;

#if CCDEBUG
                if (Control.Settings.DebugInfo.HasFlag(DType.ComponentInstall))
                {
                    if (changes.Count == 1)
                        Control.LogDebug(DType.ComponentInstall, $"-- no replace");
                    else
                        foreach (var replace in changes)
                        {
                            if (replace is AddChange add)
                                Control.LogDebug(DType.ComponentInstall,
                                    $"-- add {add.item.ComponentRef.ComponentDefID}");

                            else if (replace is RemoveChange remove)
                                Control.LogDebug(DType.ComponentInstall,
                                    $"-- remove {remove.item.ComponentRef.ComponentDefID}");

                        }
                }
#endif


                Control.LogDebug(DType.ComponentInstall, $"- adjusting {changes.Count} changes");

                //???
                int num = changes.Count;
                for (int i = 0; i < changes.Count; i++)
                {
                    if (changes[i] is IAdjustChange adj)
                    {
                        bool skip = false;
                        string cid = adj.ChangeID;
                        for (int j = i + 1; j < changes.Count; j++)
                        {
                            if(changes[j] is IAdjustChange adj2 && adj2.ChangeID == cid)
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip)
                            continue;
                        else
                            adj.DoAdjust(mechlab_helper, location_helper, changes);
                    }
                    else if (changes[i] is AddChange add)
                    {
                        Control.LogDebug(DType.ComponentInstall, $"-- add: {add.item.ComponentRef.ComponentDefID}");

                        foreach (var adj_validator in add.item.ComponentRef.GetComponents<IAdjustValidateDrop>())
                            changes.AddRange(adj_validator.ValidateDropOnAdd(add.item, location_helper, mechlab_helper,
                                changes));
                    }
                    else if (changes[i] is RemoveChange remove)
                    {
                        Control.LogDebug(DType.ComponentInstall, $"-- remove: {remove.item.ComponentRef.ComponentDefID}");

                        foreach (var adj_validator in remove.item.ComponentRef.GetComponents<IAdjustValidateDrop>())
                            changes.AddRange(adj_validator.ValidateDropOnRemove(remove.item, location_helper,
                                mechlab_helper, changes));
                    }
                }

                foreach (var validator in Validator.adjust_validators)
                {
                    var inv = GetInventory(___mechLab, changes);
                    changes.AddRange(validator(dragItem, location_helper, mechlab_helper, inv));
                }

                List<InvItem> new_inventory = GetInventory(___mechLab, changes);


                Control.LogDebug(DType.ComponentInstall, $"- post validation");

                foreach (var pst_validator in Validator.GetPost(newComponentDef))
                {
                    int n = changes.Count;
                    if (do_cancel(pst_validator(dragItem, ___mechLab.activeMechDef, new_inventory, changes), changes))
                        return false;
                    if (changes.Count != n)
                        new_inventory = GetInventory(___mechLab, changes);
                }
                Control.LogDebug(DType.ComponentInstall, $"- apply changes");

                foreach (IApplyChange change in changes)
                {
                    change?.DoChange(mechlab_helper, location_helper);
                }


                ___mechLab.ClearDragItem(true);
                __instance.RefreshHardpointData();
                ___mechLab.ValidateLoadout(false);

                return false;
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }

            return true;
        }

        private static List<InvItem> GetInventory(MechLabPanel ___mechLab, List<IChange> changes)
        {
            List<InvItem> new_inventory = ___mechLab.activeMechInventory
                .Select(i => new InvItem { item = i, location = i.MountedLocation }).ToList();

            new_inventory.AddRange(changes.OfType<AddChange>()
                .Select(i => new InvItem { item = i.item.ComponentRef, location = i.location }));

            foreach (var remove in changes.OfType<RemoveChange>())
                new_inventory.RemoveAll(i => i.item == remove.item.ComponentRef);

            return new_inventory;
        }
    }
}