using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BattleTech;
using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabItemSlotElement), "RefreshItemColor")]
    public static class MechLabSlotItem_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(MechLabItemSlotElement __instance, UIColorRefTracker ___backgroundColor,
            GameObject ___fixedEquipmentOverlay, IMechLabDropTarget ___dropParent)
        {

            ___backgroundColor.SetColor(__instance.ComponentRef);



            if (__instance.ComponentRef != null && __instance.ComponentRef.IsFixed)
            {
                var preinstalled = false;
                if (___dropParent is MechLabLocationWidget widget)
                {
                    var helper = new LocationHelper(widget);
                    preinstalled = __instance.ComponentRef.IsModuleFixed(helper.mechLab.activeMechDef);
                }


                if (!Control.Settings.UseDefaultFixedColor)
                {
                    ___fixedEquipmentOverlay.SetActive(true);
                    var color_tracker = ___fixedEquipmentOverlay.GetComponent<UIColorRefTracker>();
                    color_tracker.colorRef.UIColor = UIColor.Custom;
                    color_tracker.colorRef.color = preinstalled ? Control.Settings.PreinstalledOverlayColor : Control.Settings.DefaultFlagOverlayColor;
                    color_tracker.RefreshUIColors();
                }
                else
                    ___fixedEquipmentOverlay.SetActive(preinstalled);
            }
            else
            {
                ___fixedEquipmentOverlay.SetActive(false);
            }

            return false;
        }
    }
}
