using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(InventoryDataObject_InventoryGear), "RefreshItemColor")]
public class InventoryDataObject_InventoryGear_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryDataObject_InventoryGear __instance, InventoryItemElement theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentRef.Def, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentRef.Def, theWidget);

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(InventoryDataObject_InventoryWeapon), "RefreshItemColor")]
public class InventoryDataObject_InventoryWeapon_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryDataObject_InventoryWeapon __instance, InventoryItemElement theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentRef.Def, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentRef.Def, theWidget);

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(InventoryItemElement), "RefreshItemColor")]
public class InventoryItemElement_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryItemElement __instance, MechComponentRef ___componentRef)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(___componentRef.Def, __instance);
        TColorExtentions.ChangeTextIconColor(___componentRef.Def, __instance);

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(InventoryItemElement_NotListView), "RefreshItemColor")]
public class InventoryItemElement_NotListView_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryItemElement_NotListView __instance, MechComponentRef ___componentRef)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(___componentRef.Def, __instance);
        TColorExtentions.ChangeTextIconColor(___componentRef.Def, __instance);

        __runOriginal = false;
    }
}


[HarmonyPatch(typeof(ListElementController_InventoryGear_NotListView), "RefreshItemColor")]
public class ListElementController_InventoryGear_NotListView_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ListElementController_InventoryGear_NotListView __instance, InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentRef.Def, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentRef.Def, theWidget);

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(ListElementController_InventoryWeapon_NotListView), "RefreshItemColor")]
public class ListElementController_InventoryWeapon_NotListViewn_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ListElementController_InventoryWeapon_NotListView __instance, InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentRef.Def, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentRef.Def, theWidget);

        __runOriginal = false;
    }
}