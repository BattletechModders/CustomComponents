using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabPanel), "LoadMech")]
    public static class MechLabPanel_LoadMech_Patches
    {
        public static void Postfix(MechLabPanel __instance)
        {
            DefaultHelper.SetMechLab(__instance);
        }
    }

    [HarmonyPatch(typeof(MechLabPanel), "ExitMechLab")]
    public static class MechLabPanel_ExitMechLab_Patches
    {
        public static void Postfix()
        {
            DefaultHelper.SetMechLab(null);
        }
    }
}