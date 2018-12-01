using BattleTech;
using BattleTech.UI;
using Harmony;
using System;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechComponentDef), "GetUIColor")]
    internal static class MechComponentDef_GetUIColor
    {

        [HarmonyPostfix]
        public static void Postfix(MechComponentDef componentDef,
            ref UIColor __result)
        {
            try
            {
                if (componentDef.Is<Flags>(out var f))
                {
                    if (f.Invalid)
                    {
                        __result = UIColor.Red;
                    }
                    else if (f.Default)
                    {
                        __result = Control.Settings.DefaultFlagBackgroundColor;
                    }
                }
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }

        }
    }
}
