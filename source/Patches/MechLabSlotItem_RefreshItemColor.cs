using System;
using System.Collections.Generic;
using System.Text;
using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabItemSlotElement), "RefreshItemColor")]
    public static class MechLabSlotItem_RefreshItemColor
    {
        [HarmonyPostfix]
        public static void ChangeColor(MechLabItemSlotElement __instance, GameObject ___fixedEquipmentOverlay, IMechLabDropTarget ___dropParent)
        {
            if (__instance.ComponentRef != null && __instance.ComponentRef.IsFixed)
            {
                var preinstalled = false;
                if (___dropParent is MechLabLocationWidget widget)
                {
                    var helper = new LocationHelper(widget);
                    preinstalled = __instance.ComponentRef.IsModuleFixed(helper.mechLab.activeMechDef);
                }
                
                var color_tracker = ___fixedEquipmentOverlay.GetComponent<UIColorRefTracker>();

                if (Control.Settings.UseDefaultFixedColor)
                {
                    if (!preinstalled)
                    {
                        ___fixedEquipmentOverlay.SetActive(false);
                    }

                    return;
                }

                color_tracker.colorRef.UIColor = UIColor.Custom;
                color_tracker.colorRef.color = preinstalled ? Control.Settings.PreinstalledOverlayColor : Control.Settings.DefaultFlagOverlayColor;
                color_tracker.RefreshUIColors();
            }
        }
    }
}
