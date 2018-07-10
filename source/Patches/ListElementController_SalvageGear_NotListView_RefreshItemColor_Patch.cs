using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(ListElementController_SalvageGear_NotListView), "RefreshItemColor")]
    public static class ListElementController_SalvageGear_NotListView_RefreshItemColor_Patch
    {
        public static bool Prefix(InventoryItemElement theWidget, ListElementController_SalvageGear_NotListView __instance)
        {
            if (__instance.salvageDef.MechComponentDef.Is<ColorComponent>(out var color))
            {
                var uicolor = color.Color;
                foreach (UIColorRefTracker uicolorRefTracker in theWidget.iconBGColors)
                {
                    uicolorRefTracker.SetUIColor(uicolor);
                }

                return false;
            }
            return true;
        }
    }
}