using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(ListElementController_SalvageGear), "RefreshItemColor")]
    public static class ListElementController_SalvageGear_RefreshItemColor_Patch
    {
        public static bool Prefix(InventoryItemElement theWidget, ListElementController_SalvageGear __instance)
        {
            if (__instance.salvageDef.MechComponentDef == null || __instance.salvageDef.Description == null)
                return true;
            try
            {
                var color = Database.GetCustomComponent<ColorComponent>(__instance.salvageDef.Description.Id);

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
                Control.Logger.LogError("Salvage coloring problem!", e);
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(ListElementController_ShopGear), "RefreshItemColor")]
    public static class ListElementController_ShopGear_RefreshItemColor_Patch
    {
        public static bool Prefix(InventoryItemElement theWidget, ListElementController_ShopGear __instance)
        {
            if (__instance.componentDef == null )
                return true;
            try
            {
                var color = Database.GetCustomComponent<ColorComponent>(__instance.componentDef.Description.Id);

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
                Control.Logger.LogError("Salvage coloring problem!", e);
            }

            return true;
        }
    }
}