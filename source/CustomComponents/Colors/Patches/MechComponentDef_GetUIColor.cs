using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechComponentDef), "GetUIColor")]
internal static class MechComponentDef_GetUIColor
{

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(MechComponentDef componentDef,
        ref UIColor __result)
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
}