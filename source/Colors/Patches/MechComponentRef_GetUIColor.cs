using System;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechComponentRef), "GetUIColor")]
internal static class MechComponentRef_GetUIColor
{

    [HarmonyPostfix]
    public static void Postfix(MechComponentRef __instance,
        ref UIColor __result,
        MechComponentRef componentRef)
    {
        try
        {
            var f = componentRef.Flags<CCFlags>();
            if (f.Invalid)
            {
                __result = Control.Settings.InvalidFlagBackgroundColor;
            }
            else if (f.Default)
            {
                __result = Control.Settings.DefaultFlagBackgroundColor;
            }
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }

    }
}