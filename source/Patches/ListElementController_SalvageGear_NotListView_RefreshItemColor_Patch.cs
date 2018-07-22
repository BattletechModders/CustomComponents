using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(ListElementController_SalvageGear_NotListView), "RefreshItemColor")]
    public static class ListElementController_SalvageGear_NotListView_RefreshItemColor_Patch
    {
        public static bool Prefix(InventoryItemElement theWidget, ListElementController_SalvageGear_NotListView __instance)
        {
            if (__instance.salvageDef.MechComponentDef == null)
                return true;
            try
            {
                if (__instance.salvageDef.MechComponentDef.Is<ColorComponent>(out var color))
                {
                    var uicolor = color.UIColor;
                    foreach (UIColorRefTracker uicolorRefTracker in theWidget.iconBGColors)
                    {
                        uicolorRefTracker.SetUIColor(uicolor);
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Control.Logger.LogError("Salvage coloring problem!", e);
            }

            return true;
        }
    }
}