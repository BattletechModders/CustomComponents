using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget))]
[HarmonyPatch("RefreshHardpointData")]
public static class MechLabLocationWidget_RefreshHardpointData
{
        
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (MechLabHelper.CurrentMechLab == null || !MechLabHelper.CurrentMechLab.InMechLab)
        {
            __runOriginal = false;
            return;
        }

        var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);
        if(lhelper != null)
            lhelper.RefreshHardpoints();

        __runOriginal = false;
    }
}