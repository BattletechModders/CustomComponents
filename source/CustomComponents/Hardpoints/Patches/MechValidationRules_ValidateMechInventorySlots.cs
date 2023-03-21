using BattleTech;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechValidationRules))]
[HarmonyPatch("ValidateMechInventorySlots")]
public static class MechValidationRules_ValidateMechInventorySlots
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal)
    {
        __runOriginal = false;
    }
}