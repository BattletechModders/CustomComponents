using System;
using System.Linq;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;
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

                bool do_cancel(string error)
                {
                    if (string.IsNullOrEmpty(error))
                        return false;

                    Control.LogDebug(DType.ComponentInstall, $"- Canceled: {error}");

                    ___mechLab.ForceItemDrop(dragItem);
                    ___mechLab.OnDrop(eventData);
                    ___mechLab.ShowDropErrorMessage(new Text(error));
                    return true;
                }

                Control.LogDebug(DType.ComponentInstall, $"- pre validation");

                foreach (var pre_validator in Validator.GetPre(newComponentDef))
                {

                    if (do_cancel(pre_validator(dragItem, location)))
                        return false;
                }

                Control.LogDebug(DType.ComponentInstall, $"- replace validation");

                var changes = new Queue<IChange>();

                changes.Enqueue(new ChangeAdd(dragItem, __instance.loadout.Location));

                foreach (ReplaceValidateDropDelegate rep_validator in Validator.GetReplace(newComponentDef))
                    if (do_cancel(rep_validator(dragItem, location, changes)))
                        return false;



#if CCDEBUG
                if (Control.Settings.DebugInfo.HasFlag(DType.ComponentInstall))
                {
                    if (changes.Count == 1)
                        Control.LogDebug(DType.ComponentInstall, $"-- no replace");
                    else
                        foreach (var replace in changes)
                        {
                            if (replace is ChangeAdd add)
                                Control.LogDebug(DType.ComponentInstall,
                                    $"-- add {add.ItemID} to {add.Location}");

                            else if (replace is ChangeRemove remove)
                                Control.LogDebug(DType.ComponentInstall,
                                    $"-- remove {remove.ItemID} from {remove.Location}");

                        }
                }
#endif


                Control.LogDebug(DType.ComponentInstall, $"- adjusting");

                var state = new InventoryOperationState(changes, ___mechLab.activeMechDef);
                state.DoChanges();
                var inv = state.Inventory;

                Control.LogDebug(DType.ComponentInstall, $"- post validation");

                foreach (var pst_validator in Validator.GetPost(newComponentDef))
                {
                    int n = changes.Count;
                    if (do_cancel(pst_validator(dragItem, state.Inventory)))
                        return false;
                }

                var to_execute = state.GetResults();

                Control.LogDebug(DType.ComponentInstall, $"- apply changes");
                if(Control.Settings.DebugInfo.HasFlag(DType.ComponentInstall))
                    foreach (var change in to_execute)
                    {
                        Control.LogDebug(DType.ComponentInstall, $"-- " + change.ToString());
                    }


                foreach (var change in to_execute)
                {
                    change.ApplyToMechlab();
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

    }
}