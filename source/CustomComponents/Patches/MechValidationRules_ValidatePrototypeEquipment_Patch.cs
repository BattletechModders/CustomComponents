using BattleTech;

namespace CustomComponents.Patches;

// This is not checked in MechLab, so no point in still having it. CC validation should be used anyway.
[HarmonyPatch(typeof(MechValidationRules), nameof(MechValidationRules.ValidatePrototypeEquipment))]
public class MechValidationRules_ValidatePrototypeEquipment_Patch
{
    [HarmonyPrefix]
    public static void Prefix(ref bool __runOriginal)
    {
        if (!__runOriginal)
        {
            return;
        }

        __runOriginal = false;
    }
}
