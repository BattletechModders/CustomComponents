using System;
using BattleTech;
using BattleTech.UI;
using Harmony;
using Localize;

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
            Control.LogDebug(DType.ComponentInstall, $"OnItemGrab.Prefix {item.ComponentRef.ComponentDefID}");

            foreach(var grab_handler in item.ComponentRef.Def.GetComponents<IOnItemGrab>())
            {
                if(!grab_handler.OnItemGrab(item, ___mechLab, out var error))
                {
                    if (!string.IsNullOrEmpty(error))
                        ___mechLab.ShowDropErrorMessage(new Text(error));
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        public static void Postfix(IMechLabDraggableItem item, ref bool __result, MechComponentRef __state, MechLabPanel ___mechLab, MechLabLocationWidget __instance)
        {
            if (!__result)
                return;

            foreach (var grab_handler in item.ComponentRef.Def.GetComponents<IOnItemGrabbed>())
            {
                grab_handler.OnItemGrabbed(item, ___mechLab, __instance);
            }

            ___mechLab.ValidateLoadout(false);
        }
    }
}