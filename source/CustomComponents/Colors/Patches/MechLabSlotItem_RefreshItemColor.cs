﻿using BattleTech;
using BattleTech.UI;
using SVGImporter;
using UnityEngine;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabItemSlotElement), "RefreshItemColor")]
public static class MechLabSlotItem_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabItemSlotElement __instance, UIColorRefTracker ___backgroundColor,
        GameObject ___fixedEquipmentOverlay, IMechLabDropTarget ___dropParent,
        UIColorRefTracker ___nameTextColor, UIColorRefTracker ___iconColor, SVGImage ___icon)
    {
        if (!__runOriginal)
        {
            return;
        }

        ___backgroundColor.SetColor(__instance.ComponentRef);

        if (__instance.ComponentRef.DamageLevel == ComponentDamageLevel.Functional)
        {
            ___nameTextColor.SetTColor(___iconColor, __instance.ComponentRef);
        }
        else
        {
            ___iconColor.SetUIColor(UIColor.White);
        }

        if (___icon.vectorGraphics == null && Control.Settings.FixIcons &&
            !string.IsNullOrEmpty(__instance.ComponentRef.Def.Description.Icon))
        {
            var loadrequest =
                UnityGameInstance.BattleTechGame.DataManager.CreateLoadRequest();
            loadrequest.AddLoadRequest<SVGAsset>(BTLoadUtils.GetResourceType(nameof(BattleTechResourceType.SVGAsset)),
                __instance.ComponentRef.Def.Description.Icon,
                (id, icon) =>
                {
                    if (icon != null)
                    {
                        ___icon.vectorGraphics = icon;
                    }
                });
            loadrequest.ProcessRequests();
        }

        var color_tracker = ___fixedEquipmentOverlay.GetComponent<UIColorRefTracker>();
        // reset colors in case its a pooled item that was previously fixed
        color_tracker.SetUIColor(Control.Settings.DefaultOverlayColor);

        if (__instance.ComponentRef != null && __instance.ComponentRef.IsFixed)
        {
            var preinstalled = false;
            if (___dropParent is MechLabLocationWidget widget)
            {
                preinstalled = __instance.ComponentRef.IsModuleFixed((widget.parentDropTarget as MechLabPanel).activeMechDef);
            }

            if (!Control.Settings.UseDefaultFixedColor)
            {
                ___fixedEquipmentOverlay.SetActive(true);
                color_tracker.colorRef.UIColor = UIColor.Custom;
                color_tracker.colorRef.color = preinstalled
                    ? Control.Settings.PreinstalledOverlayColor
                    : Control.Settings.DefaultFlagOverlayColor;
            }
            else
            {
                ___fixedEquipmentOverlay.SetActive(preinstalled);
            }
        }
        else
        {
            ___fixedEquipmentOverlay.SetActive(false);
        }
        color_tracker.RefreshUIColors();

        __runOriginal = false;
    }
}