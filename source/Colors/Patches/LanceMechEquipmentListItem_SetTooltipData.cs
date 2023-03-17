using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(LanceMechEquipmentListItem), "SetData")]
internal static class LanceMechEquipmentListItem_SetData
{
    public static ComponentDamageLevel DamageLevel;

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(ComponentDamageLevel damageLevel)
    {
        DamageLevel = damageLevel;
    }
}


[HarmonyPatch(typeof(LanceMechEquipmentListItem), "SetTooltipData")]
internal static class LanceMechEquipmentListItem_SetTooltipData
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(LanceMechEquipmentListItem __instance,
        MechComponentDef MechDef, UIColorRefTracker ___backgroundColor, UIColorRefTracker ___itemTextColor)
    {
        ___backgroundColor.SetColor(MechDef);
        if (LanceMechEquipmentListItem_SetData.DamageLevel == ComponentDamageLevel.Functional)
            ___itemTextColor.SetTColor(null, MechDef);
    }
}