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
                Control.LogError(e);
            }

        }
    }
}
