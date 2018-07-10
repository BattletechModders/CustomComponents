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
            if (componentRef?.Def is IDefault)
            {
                __result = UIColor.DarkGray;
                return;
            }


            if (!(componentRef.Is<ColorComponent>(out var color) ) )
                return;

            __result = color.Color;
        }
    }
}
