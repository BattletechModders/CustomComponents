using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(LanceMechEquipmentListItem), "SetTooltipData")]
    public static class LanceMechEquipmentListItem_SetTooltipData
    {
        [HarmonyPostfix]
        public static void SetColor(LanceMechEquipmentListItem __instance,
            MechComponentDef MechDef, UIColorRefTracker ___backgroundColor)
        {
            ___backgroundColor.SetColor(MechDef);
        }
    }
}