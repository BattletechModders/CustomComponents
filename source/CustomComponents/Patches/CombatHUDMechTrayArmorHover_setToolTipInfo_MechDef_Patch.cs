using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(
    typeof(CombatHUDMechTrayArmorHover),
    nameof(CombatHUDMechTrayArmorHover.setToolTipInfo),
    typeof(MechDef),
    typeof(ArmorLocation)
)]
public static class CombatHUDMechTrayArmorHover_setToolTipInfo_MechDef_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.MethodReplacer(
            AccessTools.PropertyGetter(typeof(MechComponentRef), nameof(MechComponentRef.MountedLocation)),
            AccessTools.Method(typeof(CombatHUDMechTrayArmorHover_setToolTipInfo_MechDef_Patch), nameof(MountedLocation))
        );
    }

    public static ChassisLocations MountedLocation(this MechComponentRef mechComponentRef)
    {
        if (mechComponentRef.Def.CCFlags().HideFromCombat)
        {
            return ChassisLocations.None;
        }

        return mechComponentRef.MountedLocation;
    }
}