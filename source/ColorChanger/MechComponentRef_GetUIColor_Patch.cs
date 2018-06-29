using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechComponentRef), "GetUIColor")]
    public static class MechComponentRef_GetUIColor
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
}
