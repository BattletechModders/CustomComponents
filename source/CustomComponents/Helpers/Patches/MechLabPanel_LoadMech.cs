using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabPanel))]
[HarmonyPatch("LoadMech")]
public static class MechLabPanel_LoadMech
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(MechDef newMechDef, MechLabPanel __instance)
    {
        if (newMechDef != null)
        {
            MechLabHelper.EnterMechLab(__instance);
        }
    }
}