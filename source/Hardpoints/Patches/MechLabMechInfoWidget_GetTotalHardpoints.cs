using BattleTech.UI;
using ErosionBrushPlugin;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechLabMechInfoWidget))]
    [HarmonyPatch("GetTotalHardpoints")]
    public static class MechLabMechInfoWidget_GetTotalHardpoints
    {
        [HarmonyPrefix]
        public static bool ShowHardpoints()
        {
            MechLabHelper.CurrentMechLab?.RefreshHardpoints();


            return false;
        }
    }
}