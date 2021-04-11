using BattleTech;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechValidationRules))]
    [HarmonyPatch("ValidateMechInventorySlots")]
    public static class MechValidationRules_ValidateMechInventorySlots
    {
        [HarmonyPrefix]
        public static bool CancelVanilaValidation()
        {
            return false;
        }
    }
}