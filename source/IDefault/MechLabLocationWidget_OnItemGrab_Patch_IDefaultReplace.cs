using System;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{ 
    /// <summary>
    /// ItemGrab path for IDefaultReplace
    /// </summary>
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnItemGrab")]
    internal static class MechLabLocationWidget_OnItemGrab_Patch_IDefaultReplace
    {
        public static bool Prefix(IMechLabDraggableItem item, ref bool __result, MechLabPanel ___mechLab, ref MechComponentRef __state)
        {
            Control.Logger.LogDebug($"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

            __state = null;


            if(!(item.ComponentRef.Def is IDefaultRepace repace))
            {
                return true;
            }

            if (repace.DefaultID == item.ComponentRef.ComponentDefID)
            {
                ___mechLab.ShowDropErrorMessage("Cannot remove vital component");
                __result = false;
                return false;
            }

            MechComponentRef component_ref = CreateHelper.Ref(repace.DefaultID,
                item.ComponentRef.ComponentDefType, ___mechLab.dataManager);

            if (component_ref.Def == null)
            {
                Control.Logger.LogError($"Default replace {repace.DefaultID} for { item.ComponentRef.ComponentDefID} not found");
                return true;
            }

            Control.Logger.LogDebug("Default replace found");
            __state = component_ref;

            return true;
        }

        public static void Postfix(IMechLabDraggableItem item, ref bool __result, MechComponentRef __state, MechLabPanel ___mechLab, MechLabLocationWidget __instance)
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
                        DefaultHelper.DefaultReplaceBase(item.ComponentRef, __state.ComponentDefID, __instance.loadout.Location);
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