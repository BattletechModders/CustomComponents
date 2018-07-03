using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnMechLabDrop")]
    internal static class MechLabLocationWidget_OnDrop_Patch
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            Validator.ClearValidatorState();
        }
    }
}