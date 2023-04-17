using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using Localize;

namespace CustomComponents.InvalidInventory;

[HarmonyPatch(typeof(MechValidationRules), nameof(MechValidationRules.ValidateMechDef))]
internal static class MechValidationRules_ValidateMechDef_Patch
{
    [HarmonyPrepare]
    public static bool Prepare() => Control.Settings.CheckInvalidInventorySlots;

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    internal static void Postfix(MechValidationLevel validationLevel, DataManager dataManager, MechDef mechDef, ref Dictionary<MechValidationType, List<Text>> __result)
    {
        if (validationLevel == MechValidationLevel.MechLab)
        {
            MechValidationRules.ValidateMechInventorySlots(dataManager, mechDef, ref __result);
        }
    }
}