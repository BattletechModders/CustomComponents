using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using Localize;
using UnityEngine;

namespace CustomComponents.Patches;

public static class SetTooltips
{
    public static void SetTooltip(this GameObject go, List<Text> errors, string caption)
    {
        var tooltip = go.GetComponent<HBSTooltip>();
        if (tooltip != null)
        {
            var text = errors.Join(i => i.ToString(), "\n");
            var desc = new BaseDescriptionDef("tooltip", caption, text, null);

            tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(desc));
        }
    }
}

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleDamagedAlert")]
public static class MechLabMechInfoWidget_ToggleDamagedAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___damagedAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___damagedAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_Damaged);
    }

}

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleGenericAlert")]
public static class MechLabMechInfoWidget_ToggleGenericAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___genericAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___genericAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_Generic);
    }

}

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleInventoryAlert")]
public static class MechLabMechInfoWidget_ToggleInventoryAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___inventoryAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___inventoryAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_Inventory);
    }

}
[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleMissingWeaponAlert")]
public static class MechLabMechInfoWidget_ToggleMissingWeaponAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___missingWeaponAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___missingWeaponAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_MissingWeapon);
    }

}
[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleNoAmmoAlert")]
public static class MechLabMechInfoWidget_ToggleNoAmmoAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___noAmmoAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___noAmmoAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_NoAmmo);
    }

}

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleDestroyedAlert")]
public static class MechLabMechInfoWidget_ToggleDestroyedAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___destroyedAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___destroyedAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_Destroyed);
    }

}


[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleOverweightAlert")]
public static class MechLabMechInfoWidget_ToggleOverweightAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___overweightAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___overweightAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_Overweight);
    }

}

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleUnderweightAlert")]
public static class MechLabMechInfoWidget_ToggleUnderweightAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___underweightAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___underweightAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_Underweight);
    }

}

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("ToggleUnneededAmmoAlert")]
public static class MechLabMechInfoWidget_ToggleUnneededAmmoAlert
{
    [HarmonyPostfix]
    public static void SetTooltip(GameObject ___unneededAmmoAlert, List<Text> errors)
    {
        if (errors.Count > 0)
            ___unneededAmmoAlert.SetTooltip(errors, Control.Settings.ToolTips.Alert_UnneededAmmo);
    }

}