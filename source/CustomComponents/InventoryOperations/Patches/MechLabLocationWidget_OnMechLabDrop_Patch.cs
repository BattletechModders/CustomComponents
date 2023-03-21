using System.Collections.Generic;
using BattleTech.UI;
using CustomComponents.Changes;
using UnityEngine.EventSystems;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget), nameof(MechLabLocationWidget.OnMechLabDrop))]
internal static class MechLabLocationWidget_OnMechLabDrop_Patch
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance,
        MechLabPanel ___mechLab,
        PointerEventData eventData)
    {
        if (!__runOriginal)
        {
            return;
        }

            var dragItem = ___mechLab.DragItem as MechLabItemSlotElement;
            var location = __instance.loadout.Location;

            if (!___mechLab.Initialized)
            {
                __runOriginal = false;
                return;
            }

            var newComponentDef = dragItem?.ComponentRef?.Def;
            if (newComponentDef == null)
            {
                __runOriginal = false;
                return;
            }

            Log.ComponentInstall.Trace?.Log($"OnMechLabDrop: Adding {newComponentDef.Description.Id}");

            bool do_cancel(string error)
            {
                if (string.IsNullOrEmpty(error))
                {
                    return false;
                }

                Log.ComponentInstall.Trace?.Log($"- Canceled: {error}");

                ___mechLab.ForceItemDrop(dragItem);
                ___mechLab.OnDrop(eventData);
                ___mechLab.ShowDropErrorMessage(new(error));
                return true;
            }

            Log.ComponentInstall.Trace?.Log("- pre validation");

            foreach (var pre_validator in Validator.GetPre(newComponentDef))
            {

                if (do_cancel(pre_validator(dragItem, location)))
                {
                    __runOriginal = false;
                    return;
                }
            }

            Log.ComponentInstall.Trace?.Log("- replace validation");

            var changes = new Queue<IChange>();

            changes.Enqueue(new Change_Add(dragItem, __instance.loadout.Location));

            foreach (var rep_validator in Validator.GetReplace(newComponentDef))
            {
                if (do_cancel(rep_validator(dragItem, location, changes)))
                {
                    __runOriginal = false;
                    return;
                }
            }

#if DEBUG
            if (Log.ComponentInstall.Debug != null)
            {
                if (changes.Count == 1)
                {
                    Log.ComponentInstall.Debug.Log($"-- no replace");
                }
                else
                    foreach (var replace in changes)
                    {
                        if (replace is Change_Add add)
                        {
                            Log.ComponentInstall.Debug.Log($"-- add {add.ItemID} to {add.Location}");
                        }

                        else if (replace is Change_Remove remove)
                        {
                            Log.ComponentInstall.Debug.Log($"-- remove {remove.ItemID} from {remove.Location}");
                        }
                    }
            }
#endif

        Log.ComponentInstall.Trace?.Log("- adjusting");

            var state = new InventoryOperationState(changes, ___mechLab.activeMechDef);
            state.DoChanges();
            var inv = state.Inventory;

            Log.ComponentInstall.Trace?.Log("- post validation");

            foreach (var pst_validator in Validator.GetPost(newComponentDef))
            {
                var n = changes.Count;
                if (do_cancel(pst_validator(dragItem, state.Inventory)))
                {
                    __runOriginal = false;
                    return;
                }
            }


            state.ApplyMechlab();


            ___mechLab.ClearDragItem(true);
            __instance.RefreshHardpointData();
            ___mechLab.ValidateLoadout(false);

            __runOriginal = false;
    }

}