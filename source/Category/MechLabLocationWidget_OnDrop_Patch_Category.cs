using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using TMPro;
using UnityEngine.EventSystems;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnMechLabDrop")]
    internal static class MechLabLocationWidget_OnDrop_Patch_Category
    {
        public static bool Prefix(MechLabLocationWidget __instance, ref string ___dropErrorMessage,
            List<MechLabItemSlotElement> ___localInventory,
            int ___usedSlots,
            int ___maxSlots,
            TextMeshProUGUI ___locationName,
            MechLabPanel ___mechLab,
            PointerEventData eventData)
        {
            var error_message = ___dropErrorMessage;
            var drag_item = ___mechLab.DragItem;

            void cancel_drop()
            {
                ___mechLab.ShowDropErrorMessage(error_message);
                ___mechLab.OnDrop(eventData);
            }

            void complete_drop()
            {
                var clear = __instance.OnAddItem(drag_item, true);
                if (__instance.Sim != null)
                {
                    WorkOrderEntry_InstallComponent subEntry = __instance.Sim.CreateComponentInstallWorkOrder(
                        ___mechLab.baseWorkOrder.MechID,
                        drag_item.ComponentRef, __instance.loadout.Location, drag_item.MountedLocation);
                    ___mechLab.baseWorkOrder.AddSubEntry(subEntry);
                }

                drag_item.MountedLocation = __instance.loadout.Location;
                ___mechLab.ClearDragItem(clear);
                __instance.RefreshHardpointData();
                ___mechLab.ValidateLoadout(false);
            }


            if (!Control.settings.LoadDefaultValidators)
                return true;

            if (!___mechLab.Initialized)
            {
                return false;
            }


            if (drag_item?.ComponentRef == null)
            {
                return false;
            }

            var flag = __instance.ValidateAdd(drag_item.ComponentRef);
            if (!flag)
            {
                cancel_drop();
                return false;
            }

            if (!(drag_item.ComponentRef.Def is ICategory item))
            {
                complete_drop();
                return false;
            }

            var state = Validator.GetState<CategoryValidatorState>();
            if (state == null || state.descriptor.Name != item.CategoryID || state.ReplacementIndex < 0)
            {
                complete_drop();
                return false;
            }

            var old_item = ___localInventory[state.ReplacementIndex];
            __instance.OnRemoveItem(old_item, true);
            ___mechLab.ForceItemDrop(old_item);

            complete_drop();

            return false;
        }

    }
}