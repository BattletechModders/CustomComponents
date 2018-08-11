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
            try
            {
                var color = __instance.salvageDef.MechComponentDef?.GetComponent<ColorComponent>();

                if (color != null)
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
                Control.Logger.LogError(e);
            }

            return true;
        }
    }
}