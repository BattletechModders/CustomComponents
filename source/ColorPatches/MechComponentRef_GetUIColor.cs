using BattleTech;
using BattleTech.UI;
using Harmony;
using System;

namespace CustomComponents
{
    //[HarmonyPatch(typeof(MechComponentRef), "GetUIColor")]
    //internal static class MechComponentRef_GetUIColor
    //{

    //    [HarmonyPostfix]
    //    public static void Postfix(MechComponentRef __instance,
    //        ref UIColor __result,
    //        MechComponentRef componentRef)
    //    {
    //        try
    //        {
    //            if (componentRef.Is<Flags>(out var f))
    //            {
    //                if (f.Invalid)
    //                {
    //                    __result = UIColor.Red;
    //                }
    //                else if (f.Default)
    //                {
    //                    __result = Control.Settings.DefaultFlagBackgroundColor;
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Control.LogError(e);
    //        }

    //    }
    //}
}
