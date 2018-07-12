using BattleTech;
using BattleTech.UI;
using Harmony;
using System;

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
            try
            {
                if (componentRef.Is<ColorComponent>(out var color))
                {
                    __result = color.UIColor;
                    return;
                }

                if (componentRef.Is<Flags>(out var f) && f.Default)
                {
                    __result = UIColor.DarkGray;
                    return;
                }
            }
            catch(Exception e)
            {
                Control.Logger.LogError(e);
            }

        }
    }
}
