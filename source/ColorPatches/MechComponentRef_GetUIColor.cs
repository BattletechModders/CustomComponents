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
                var f = componentRef.Flags();
                if (f["invalid"])
                {
                    __result = UIColor.Red;
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
