using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(InventoryDataObject_SalvageGear), "RefreshItemColor")]
    public static class InventoryDataObject_SalvageGear_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(InventoryDataObject_SalvageGear __instance, InventoryItemElement theWidget)
        {
            try
            {
                if (__instance.componentDef == null)
                    return true;

                theWidget.iconBGColors.SetColor(__instance.componentDef);
                return false;

            }
            catch (Exception e)
            {
                Control.LogError(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryDataObject_SalvageWeapon), "RefreshItemColor")]
    public static class InInventoryDataObject_SalvageWeapon_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(InventoryDataObject_SalvageWeapon __instance, InventoryItemElement theWidget)
        {
            try
            {
                if (__instance.componentDef == null)
                    return true;

                theWidget.iconBGColors.SetColor(__instance.componentDef);

                return false;

            }
            catch (Exception e)
            {
                Control.LogError(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ListElementController_SalvageGear_NotListView), "RefreshItemColor")]
    public static class ListElementController_SalvageGear_NotListView_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(ListElementController_SalvageGear_NotListView __instance,
            InventoryItemElement_NotListView theWidget)
        {
            try
            {
                if (__instance.componentDef == null)
                    return true;

                theWidget.iconBGColors.SetColor(__instance.componentDef);
                return false;

            }
            catch (Exception e)
            {
                Control.LogError(e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ListElementController_SalvageWeapon_NotListView), "RefreshItemColor")]
    public static class ListElementController_SalvageWeapon_NotListView_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(ListElementController_SalvageWeapon_NotListView __instance,
            InventoryItemElement_NotListView theWidget)
        {
            try
            {
                if (__instance.componentDef == null)
                    return true;
                theWidget.iconBGColors.SetColor(__instance.componentDef);

                return false;
            }
            catch (Exception e)
            {
                Control.LogError(e);
                return true;
            }
        }
    }
}