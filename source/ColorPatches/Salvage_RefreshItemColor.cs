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

                ColorExtentions.ChangeBackColor(__instance.componentDef, theWidget);
                TColorExtentions.ChangeTextIconColor(__instance.componentDef, theWidget);

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
    public static class InventoryDataObject_SalvageWeapon_RefreshItemColor
    {
        [HarmonyPrefix]
        public static bool ChangeColor(InventoryDataObject_SalvageWeapon __instance, InventoryItemElement theWidget)
        {
            try
            {
                if (__instance.componentDef == null)
                    return true;

                ColorExtentions.ChangeBackColor(__instance.weaponDef ?? __instance.componentDef, theWidget);
                TColorExtentions.ChangeTextIconColor(__instance.weaponDef ?? __instance.componentDef, theWidget);

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

                ColorExtentions.ChangeBackColor(__instance.componentDef, theWidget);
                TColorExtentions.ChangeTextIconColor(__instance.componentDef, theWidget);
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
                ColorExtentions.ChangeBackColor(__instance.weaponDef ?? __instance.componentDef, theWidget);
                TColorExtentions.ChangeTextIconColor(__instance.weaponDef ?? __instance.componentDef, theWidget);

                return false;
            }
            catch (Exception e)
            {
                Control.LogError(e);
                return true;
            }
        }
    }


    [HarmonyPatch(typeof(InventoryDataObject_SalvageFullMech), "RefreshItemColor")]
    public static class InInventoryDataObject_SalvageFullMech_RefreshItemColor
    {
        [HarmonyPrefix]
        public static void ChangeColor(InventoryDataObject_SalvageFullMech __instance, InventoryItemElement theWidget)
        {
            try
            {
                TColorExtentions.ResetTextIconColor(theWidget);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(ListElementController_SalvageFullMech_NotListView), "RefreshItemColor")]
    public static class ListElementController_SalvageFullMech_NotListView_RefreshItemColor
    {
        [HarmonyPrefix]
        public static void ChangeColor(ListElementController_SalvageFullMech_NotListView __instance,
            InventoryItemElement_NotListView theWidget)
        {
            try
            {
                TColorExtentions.ResetTextIconColor(theWidget);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryDataObject_SalvageMechPart), "RefreshItemColor")]
    public static class InInventoryDataObject_SalvageMechPart_RefreshItemColor
    {
        [HarmonyPrefix]
        public static void ChangeColor(InventoryDataObject_SalvageMechPart __instance, InventoryItemElement theWidget)
        {
            try
            {
                TColorExtentions.ResetTextIconColor(theWidget);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(ListElementController_SalvageMechPart_NotListView), "RefreshItemColor")]
    public static class ListElementController_SalvageMechPart_NotListView_RefreshItemColor
    {
        [HarmonyPrefix]
        public static void ChangeColor(ListElementController_SalvageMechPart_NotListView __instance,
            InventoryItemElement_NotListView theWidget)
        {
            try
            {
                TColorExtentions.ResetTextIconColor(theWidget);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }

}