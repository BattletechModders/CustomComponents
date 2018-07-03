using System.Security.Permissions;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechComponentRef), "GetUIColor")]
    internal static class MechComponentRef_GetUIColor
    {

        [HarmonyPostfix]
        public static void Postfix(MechComponentRef __instance,
            ref UIColor __result,
            MechComponentRef componentRef)
        {
            if (componentRef == null || componentRef.Def == null || !(componentRef.Def is IColorComponent) )
                return;

            var color_info = componentRef.Def as IColorComponent;
            __result = color_info.Color;
        }
    }


    //[HarmonyPatch(typeof(ListElementController_ShopGear), "RefreshItemColor")]
    //public static class ListElementController_ShopGear_RefreshItemColor_Patch
    //{
    //    public static bool Prefix(InventoryItemElement theWidget, ListElementController_ShopGear __instance)
    //    {
    //        if (__instance.shopDefItem. is IColorComponent)
    //        {
    //            var uicolor = (__instance.salvageDef as IColorComponent).Color;
    //            foreach (UIColorRefTracker uicolorRefTracker in theWidget.iconBGColors)
    //            {
    //                uicolorRefTracker.SetUIColor(uicolor);
    //            }

    //            return false;
    //        }
    //        return true;
    //    }
    //}

}
