using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(InventoryDataObject_ShopWeapon), "RefreshItemColor")]
    public static class InventoryDataObject_ShopWeapon_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(InventoryItemElement theWidget, InventoryDataObject_ShopWeapon __instance)
        {
            try
            {
                theWidget.iconBGColors.SetColor(__instance.weaponDef);
            }
            catch (Exception ex)
            {
                Control.LogError(ex);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryDataObject_ShopGear), "RefreshItemColor")]
    public static class IInventoryDataObject_ShopGear_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(InventoryItemElement theWidget, InventoryDataObject_ShopGear __instance)
        {
            try
            {
                theWidget.iconBGColors.SetColor(__instance.componentDef);
            }
            catch (Exception ex)
            {
                Control.LogError(ex);
            }
            return false;
        }
    }
}