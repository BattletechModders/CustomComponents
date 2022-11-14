using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(InventoryDataObject_ShopWeapon), "RefreshItemColor")]
    public static class InventoryDataObject_ShopWeapon_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(InventoryItemElement theWidget, InventoryDataObject_ShopWeapon __instance)
        {
            try
            {
                ColorExtentions.ChangeBackColor(__instance.weaponDef, theWidget);
                TColorExtentions.ChangeTextIconColor(__instance.weaponDef, theWidget);
            }
            catch (Exception ex)
            {
                Log.Main.Error?.Log(ex);
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
                ColorExtentions.ChangeBackColor(__instance.componentDef, theWidget);
                TColorExtentions.ChangeTextIconColor(__instance.componentDef, theWidget);
            }
            catch (Exception ex)
            {
                Log.Main.Error?.Log(ex);
            }
            return false;
        }
    }
}