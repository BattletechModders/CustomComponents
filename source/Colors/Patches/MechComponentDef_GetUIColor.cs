using BattleTech;
using BattleTech.UI;
using Harmony;
using System;

namespace CustomComponents.Patches
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
                var f = componentDef.Flags<CCFlags>();
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
}
