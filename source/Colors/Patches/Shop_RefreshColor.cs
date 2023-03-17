using System;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(InventoryDataObject_ShopWeapon), "RefreshItemColor")]
public static class InventoryDataObject_ShopWeapon_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryItemElement theWidget, InventoryDataObject_ShopWeapon __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        try
        {
            ColorExtentions.ChangeBackColor(__instance.weaponDef, theWidget);
            TColorExtentions.ChangeTextIconColor(__instance.weaponDef, theWidget);
        }
        catch (Exception ex)
        {
            Log.Main.Error?.Log(ex);
        }

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(InventoryDataObject_ShopGear), "RefreshItemColor")]
public static class IInventoryDataObject_ShopGear_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryItemElement theWidget, InventoryDataObject_ShopGear __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentDef, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentDef, theWidget);

        __runOriginal = false;
    }
}