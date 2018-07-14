using System;
using System.Linq;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;

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
                bool do_cancel(string error)
                {
                    if (string.IsNullOrEmpty(error))
                        return false;

                    Control.Logger.LogDebug($"- Canceled: {error}");

                    ___mechLab.ShowDropErrorMessage(error);
                    ___mechLab.OnDrop(eventData);
                    return true;
                }

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

                Control.Logger.LogDebug($"OnMechLabDrop: Adding {newComponentDef.Description.Id}");

                var location_helper = new LocationHelper(__instance);
                var mechlab_helper = new MechLabHelper(___mechLab);

                Control.Logger.LogDebug($"- pre validation");

                foreach (var pre_validator in Validator.GetPre(newComponentDef))
                    if (do_cancel(pre_validator(dragItem, location_helper, mechlab_helper)))
                        return false;

                Control.Logger.LogDebug($"- replace validation");

                MechLabItemSlotElement replaceItem = null;
                foreach (var rep_validator in Validator.GetReplace(newComponentDef))
                    if (do_cancel(rep_validator(dragItem, location_helper, ref replaceItem)))
                        return false;

                if(replaceItem == null)
                    Control.Logger.LogDebug($"-- no replace");
                else
                    Control.Logger.LogDebug($"-- replace {replaceItem.ComponentRef.ComponentDefID}");


                List<IChange> changes = new List<IChange>();
                changes.Add(new AddChange(__instance.loadout.Location, dragItem));
                if (replaceItem != null)
                    changes.Add(new RemoveChange(__instance.loadout.Location, replaceItem));



                Control.Logger.LogDebug($"- adjust for drop item ");

                foreach (var adj_validator in newComponentDef.GetComponents<IAdjustValidateDrop>())
                    changes.AddRange(adj_validator.ValidateDropOnAdd(dragItem, location_helper, mechlab_helper));

                if (replaceItem != null)
                {
                    Control.Logger.LogDebug($"- adjust for replaced item ");
                    foreach (var adj_validator in replaceItem.ComponentRef.Def.GetComponents<IAdjustValidateDrop>())
                        changes.AddRange(adj_validator.ValidateDropOnRemove(replaceItem, location_helper, mechlab_helper));
                }

                List<InvItem> new_inventory = ___mechLab.activeMechInventory
                    .Select(i => new InvItem { item = i, location = i.MountedLocation }).ToList();

                new_inventory.AddRange(changes.OfType<AddChange>()
                    .Select(i => new InvItem { item = i.item.ComponentRef, location = i.location }));

                foreach(var remove in changes.OfType<RemoveChange>())
                {
                    new_inventory.RemoveAll(i => i.item == remove.item.ComponentRef);
                }

                Control.Logger.LogDebug($"- post validation");

                foreach (var pst_validator in Validator.GetPost(newComponentDef))
                    if(do_cancel(pst_validator(dragItem, ___mechLab.activeMechDef, new_inventory, changes)))
                        return false;



                Control.Logger.LogDebug($"- apply changes");

                foreach (var change in changes)
                {
                    change.DoChange(mechlab_helper, location_helper);
                }

                ___mechLab.ClearDragItem(true);
                __instance.RefreshHardpointData();
                ___mechLab.ValidateLoadout(false);

                return false;
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }

            return true;
        }
    }
}