using System;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Category
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    internal static class MechLabLocationWidget_OnItemGrab_Patch_Category
    {

        public static bool Prefix(IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab, ref MechComponentRef __state)
        {
            Control.Logger.LogDebug($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

            __state = null;

            if (!(item.ComponentRef.Def is ICategory cat_item))
            {
                return true;
            }

            if (!cat_item.CategoryDescriptor.AllowRemove)
            {
                ___mechLab.ShowDropErrorMessage("Cannot remove vital component");
                __result = false;
                return false;
            }

            if (string.IsNullOrEmpty(cat_item.CategoryDescriptor.DefaultReplace))
                return true;

            if (cat_item.CategoryDescriptor.DefaultReplace == item.ComponentRef.ComponentDefID)
            {
                ___mechLab.ShowDropErrorMessage("Cannot remove vital component");
                __result = false;
                return false;
            }

            MechComponentRef component_ref = CreateHelper.Ref(cat_item.CategoryDescriptor.DefaultReplace,
                item.ComponentRef.ComponentDefType, ___mechLab.dataManager);

            if (component_ref.Def == null)
            {
                Control.Logger.LogDebug("Default replace not found, cancel");
                __result = false;
                ___mechLab.ShowDropErrorMessage("Cannot remove vital component");

                return false;
            }
            Control.Logger.LogDebug("Default replace found");
            __state = component_ref;

            return true;
        }

        public static void Postfix(ref bool __result, MechComponentRef __state, MechLabPanel ___mechLab, MechLabLocationWidget __instance)
        {
            Control.Logger.LogDebug($"OnItemGrab.Postfix CanRemove: {__result}");
            if (__state != null)
            {
                Control.Logger.LogDebug($"OnItemGrab.Postfix Replacement received: {__state.ComponentDefID}");

                try
                {
                    var slot = CreateHelper.Slot(___mechLab, __state, __instance.loadout.Location);
                        
                    __instance.OnAddItem(slot, false);
                    ___mechLab.ValidateLoadout(false);

                    if (__instance.Sim != null)
                    {
                        WorkOrderEntry_InstallComponent subEntry = __instance.Sim.CreateComponentInstallWorkOrder(
                            ___mechLab.baseWorkOrder.MechID,
                            slot.ComponentRef, __instance.loadout.Location, slot.MountedLocation);
                        ___mechLab.baseWorkOrder.AddSubEntry(subEntry);
                    }
                }
                catch (Exception e)
                {
                    Control.Logger.LogDebug("OnItemGrab.Postfix Error:", e);
                }
            }
        }
    }
}