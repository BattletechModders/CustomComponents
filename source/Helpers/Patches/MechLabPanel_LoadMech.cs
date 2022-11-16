using BattleTech;
using BattleTech.UI;
using Harmony;


namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabPanel))]
[HarmonyPatch("LoadMech")]
public static class MechLabPanel_LoadMech
{
    [HarmonyPostfix]
    public static void InitMechLabHelper(MechDef newMechDef, MechLabPanel __instance)
    {
        if (newMechDef != null)
            MechLabHelper.EnterMechLab(__instance);
    }
}