using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabMechInfoWidget))]
[HarmonyPatch("GetTotalHardpoints")]
public static class MechLabMechInfoWidget_GetTotalHardpoints
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal)
    {
        if (!__runOriginal)
        {
            return;
        }

        MechLabHelper.CurrentMechLab?.RefreshHardpoints();

        __runOriginal = false;
    }
}