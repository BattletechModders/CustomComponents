using System;
using System.Collections;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnMechLabDrop")]
    internal static class MechLabLocationWidget_OnMechLabDrop_Patch
    {
        public static bool Prefix(MechLabLocationWidget __instance,
            List<MechLabItemSlotElement> ___localInventory,
            int ___usedSlots,
            int ___maxSlots,
            TextMeshProUGUI ___locationName,
            ref string ___dropErrorMessage,
            MechLabPanel ___mechLab,
            PointerEventData eventData)
        {
            try
            {
                var dragItem = ___mechLab.DragItem as MechLabItemSlotElement;

                Control.Logger.LogDebug($"========= Addding Item ===========");

                if (!___mechLab.Initialized)
                {
                    return false;
                }

                var newComponentDef = dragItem?.ComponentRef?.Def;
                if (newComponentDef == null)
                {
                    return false;
                }

                var validators = Validator.GetValidateDropDelegates(newComponentDef);

                var helper = new LocationHelper(__instance);

                IValidateDropResult result = null;

                Control.Logger.LogDebug($"========= begin validate ===========");
                foreach (var validator in validators)
                {
                    result = validator(dragItem, helper, result);


                    if (result == null || result.Status == ValidateDropStatus.Continue)
                    {
                        continue;
                    }

                    if (result is ValidateDropRemoveDragItem drop)
                    {
                        Control.Logger.LogDebug($"========= Interrupt: drop item ===========");

                        // remove item and delete it
                        dragItem.thisCanvasGroup.blocksRaycasts = true;
                        dragItem.MountedLocation = ChassisLocations.None;
                        ___mechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, dragItem.gameObject);
                        ___mechLab.ClearDragItem(true);
                        if (drop.ShowMessage)
                            ___mechLab.ShowDropErrorMessage(drop.Message);

                        return false;
                    }

                    if (result is ValidateDropError error)
                    {
                        Control.Logger.LogDebug($"========= Interrupt: error ===========");
                        Control.Logger.LogDebug($"{error.ErrorMessage}");

                        ___dropErrorMessage = error.ErrorMessage;
                        ___mechLab.ShowDropErrorMessage(___dropErrorMessage);
                        ___mechLab.OnDrop(eventData);
                        return false;
                    }
                }
                Control.Logger.LogDebug($"========= Validation finished ===========");

                var mech_lab_helper = new MechLabHelper(___mechLab);

                if (result is ValidateDropChange change_result)
                {
                    foreach (var change in change_result.Changes)
                        change.DoChange(mech_lab_helper, helper);
                }

                var clear = __instance.OnAddItem(dragItem, true);
                if (__instance.Sim != null)
                {
                    WorkOrderEntry_InstallComponent subEntry = __instance.Sim.CreateComponentInstallWorkOrder(
                        ___mechLab.baseWorkOrder.MechID,
                        dragItem.ComponentRef, __instance.loadout.Location, dragItem.MountedLocation);
                    ___mechLab.baseWorkOrder.AddSubEntry(subEntry);
                }

                dragItem.MountedLocation = __instance.loadout.Location;
                ___mechLab.ClearDragItem(clear);
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