using System;
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
                var location = __instance.loadout.Location;

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

                bool do_cancel(string error, IEnumerable<IChange> allchanges)
                {
                    if (string.IsNullOrEmpty(error))
                        return false;

                    Control.LogDebug(DType.ComponentInstall, $"- Canceled: {error}");

                    if (allchanges != null)
                    {
                        foreach (ICancelChange change in allchanges)
                        {
                            change.CancelChange();
                        }
                    }

                    ___mechLab.OnDrop(eventData);
                    ___mechLab.ShowDropErrorMessage(new Text(error));
                    return true;
                }

                Control.LogDebug(DType.ComponentInstall, $"- pre validation");

                foreach (var pre_validator in Validator.GetPre(newComponentDef))
                {

                    if (do_cancel(pre_validator(dragItem, location), null))
                        return false;
                }

                Control.LogDebug(DType.ComponentInstall, $"- replace validation");

                var changes = new Queue<IChange>();

                changes.Enqueue(new AddFromInventoryChange(__instance.loadout.Location, dragItem));

                foreach (ReplaceValidateDropDelegate rep_validator in Validator.GetReplace(newComponentDef))
                    if (do_cancel(rep_validator(dragItem, location, changes), changes))
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


                Control.LogDebug(DType.ComponentInstall, $"- adjusting");

                List<IChange> to_execute = new List<IChange>();
                List<InvItem> inventory = GetInventory(to_execute.Concat(changes));

                while (changes.Count > 0)
                {
                    var change = changes.Dequeue();

                    if (change is IApplyChange)
                        to_execute.Add(change);
                    
                    if (change is IDelayChange adjust)
                    {
                        if(changes.Any(i => i is IDelayChange ch2 && ch2.ChangeID == adjust.ChangeID))
                            continue;
                    }

                    if (change.DoAdjust(changes, inventory))
                        inventory = GetInventory(to_execute.Concat(changes));

                }

                Control.LogDebug(DType.ComponentInstall, $"- post validation");

                foreach (var pst_validator in Validator.GetPost(newComponentDef))
                {
                    int n = changes.Count;
                    if (do_cancel(pst_validator(dragItem, inventory), to_execute))
                        return false;
                }
                Control.LogDebug(DType.ComponentInstall, $"- apply changes");

                foreach (IApplyChange change in changes)
                {
                    change?.DoChange();
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

        private static List<InvItem> GetInventory(IEnumerable<IChange> changes)
        {
            List<InvItem> new_inventory = MechLabHelper.CurrentMechLab.MechLab.activeMechInventory
                .Select(i => new InvItem { item = i, location = i.MountedLocation }).ToList();

            foreach (IApplyChange change in changes)
            {
                change.PreviewChange(new_inventory);
            }

            return new_inventory;
        }
    }
}