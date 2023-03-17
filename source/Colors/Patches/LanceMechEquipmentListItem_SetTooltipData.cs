using System;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(LanceMechEquipmentListItem), "SetData")]
internal static class LanceMechEquipmentListItem_SetData
{
    public static ComponentDamageLevel DamageLevel;

    [HarmonyPostfix]
    public static void SetColor(ComponentDamageLevel damageLevel)
    {
        DamageLevel = damageLevel;
    }
}


[HarmonyPatch(typeof(LanceMechEquipmentListItem), "SetTooltipData")]
internal static class LanceMechEquipmentListItem_SetTooltipData
{
    [HarmonyPostfix]
    public static void SetColor(LanceMechEquipmentListItem __instance,
        MechComponentDef MechDef, UIColorRefTracker ___backgroundColor, UIColorRefTracker ___itemTextColor)
    {
        try
        {
            ___backgroundColor.SetColor(MechDef);
            if (LanceMechEquipmentListItem_SetData.DamageLevel == ComponentDamageLevel.Functional)
                ___itemTextColor.SetTColor(null, MechDef);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}