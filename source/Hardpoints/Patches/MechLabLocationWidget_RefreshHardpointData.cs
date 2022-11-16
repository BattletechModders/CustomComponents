using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget))]
[HarmonyPatch("RefreshHardpointData")]
public static class MechLabLocationWidget_RefreshHardpointData
{
        
    [HarmonyPrefix]
    public static bool RefreshHardpoints(MechLabLocationWidget __instance)
    {
        if (MechLabHelper.CurrentMechLab == null || !MechLabHelper.CurrentMechLab.InMechLab)
            return false;

        var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);
        if(lhelper != null)
            lhelper.RefreshHardpoints();

        return false;
    }
}