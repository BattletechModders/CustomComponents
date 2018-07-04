using System;
using System.Collections;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using TMPro;
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

                if (!Control.settings.LoadDefaultValidators)
                {
                    Control.Logger.LogDebug($"Addding Item: default validators not enabled");
                    return true;
                }

                if (!___mechLab.Initialized)
                {
                    return true;
                }

                var newComponentDef = dragItem?.ComponentRef?.Def;
                if (newComponentDef == null)
                {
                    return true;
                }

                var validators = Validator.GetValidateDropDelegates(newComponentDef);

                foreach (var validator in validators)
                {
                    var result = validator(dragItem, __instance);

                    Control.Logger.LogDebug($"========= Validate done ===========");

                    if (result == null)
                    {
                        continue;
                    }

                    if (result is ValidateDropReplaceItem replace)
                    {
                        Control.Logger.LogDebug($"========= ValidateDropReplaceItem ===========");

                        var element = replace.ToReplaceElement;
                        if (___usedSlots - element.ComponentRef.Def.InventorySize + newComponentDef.InventorySize <= ___maxSlots)
                        {
                            __instance.OnRemoveItem(element, true);
                            ___mechLab.ForceItemDrop(element); // side effect issue: MechLabPanel.dragItem = element
                            Traverse.Create(___mechLab).Field("dragItem").SetValue(dragItem); // fix: MechLabPanel.dragItem = dragItem
                        }
                        continue;
                    }

                    if (result is ValidateDropRemoveDragItem)
                    {
                        Control.Logger.LogDebug($"========= ValidateDropRemoveDragItem ===========");

                        // remove item and delete it
                        dragItem.thisCanvasGroup.blocksRaycasts = true;
                        dragItem.MountedLocation = ChassisLocations.None;
                        ___mechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, dragItem.gameObject);
                        ___mechLab.ClearDragItem(true);
                        return false;
                    }

                    if (result is ValidateDropError error)
                    {
                        Control.Logger.LogDebug($"========= ValidateDropError ===========");

                        ___dropErrorMessage = error.ErrorMessage;
                        ___mechLab.ShowDropErrorMessage(___dropErrorMessage);
                        ___mechLab.OnDrop(eventData);
                        return false;
                    }
                }

                Control.Logger.LogDebug($"========= ValidateDropContinue ===========");
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }

            return true;
        }
    }
}