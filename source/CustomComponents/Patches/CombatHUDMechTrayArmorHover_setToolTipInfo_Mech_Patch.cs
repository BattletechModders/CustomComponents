using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(
    typeof(CombatHUDMechTrayArmorHover),
    nameof(CombatHUDMechTrayArmorHover.setToolTipInfo),
    typeof(Mech),
    typeof(ArmorLocation)
)]
public static class CombatHUDMechTrayArmorHover_setToolTipInfo_Mech_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.MethodReplacer(
            AccessTools.PropertyGetter(typeof(MechComponent), nameof(MechComponent.Location)),
            AccessTools.Method(typeof(CombatHUDMechTrayArmorHover_setToolTipInfo_Mech_Patch), nameof(Location))
        );
    }

    public static int Location(this MechComponent mechComponent)
    {
        if (mechComponent.componentDef.CCFlags().HideFromCombat)
        {
            return 0;
        }

        return mechComponent.Location;
    }
}