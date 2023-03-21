using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(InventoryDataObject_SalvageGear), "RefreshItemColor")]
public static class InventoryDataObject_SalvageGear_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryDataObject_SalvageGear __instance, InventoryItemElement theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.componentDef == null)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentDef, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentDef, theWidget);

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(InventoryDataObject_SalvageWeapon), "RefreshItemColor")]
public static class InventoryDataObject_SalvageWeapon_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryDataObject_SalvageWeapon __instance, InventoryItemElement theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.componentDef == null)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.weaponDef ?? __instance.componentDef, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.weaponDef ?? __instance.componentDef, theWidget);

        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(ListElementController_SalvageGear_NotListView), "RefreshItemColor")]
public static class ListElementController_SalvageGear_NotListView_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ListElementController_SalvageGear_NotListView __instance,
        InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.componentDef == null)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.componentDef, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.componentDef, theWidget);
        __runOriginal = false;
    }
}

[HarmonyPatch(typeof(ListElementController_SalvageWeapon_NotListView), "RefreshItemColor")]
public static class ListElementController_SalvageWeapon_NotListView_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ListElementController_SalvageWeapon_NotListView __instance,
        InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.componentDef == null)
        {
            return;
        }

        ColorExtentions.ChangeBackColor(__instance.weaponDef ?? __instance.componentDef, theWidget);
        TColorExtentions.ChangeTextIconColor(__instance.weaponDef ?? __instance.componentDef, theWidget);

        __runOriginal = false;
    }
}


[HarmonyPatch(typeof(InventoryDataObject_SalvageFullMech), "RefreshItemColor")]
public static class InInventoryDataObject_SalvageFullMech_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryDataObject_SalvageFullMech __instance, InventoryItemElement theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        TColorExtentions.ResetTextIconColor(theWidget);
    }
}

[HarmonyPatch(typeof(ListElementController_SalvageFullMech_NotListView), "RefreshItemColor")]
public static class ListElementController_SalvageFullMech_NotListView_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ListElementController_SalvageFullMech_NotListView __instance,
        InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        TColorExtentions.ResetTextIconColor(theWidget);
    }
}

[HarmonyPatch(typeof(InventoryDataObject_SalvageMechPart), "RefreshItemColor")]
public static class InInventoryDataObject_SalvageMechPart_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, InventoryDataObject_SalvageMechPart __instance, InventoryItemElement theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        TColorExtentions.ResetTextIconColor(theWidget);
    }
}

[HarmonyPatch(typeof(ListElementController_SalvageMechPart_NotListView), "RefreshItemColor")]
public static class ListElementController_SalvageMechPart_NotListView_RefreshItemColor
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ListElementController_SalvageMechPart_NotListView __instance,
        InventoryItemElement_NotListView theWidget)
    {
        if (!__runOriginal)
        {
            return;
        }

        TColorExtentions.ResetTextIconColor(theWidget);
    }
}