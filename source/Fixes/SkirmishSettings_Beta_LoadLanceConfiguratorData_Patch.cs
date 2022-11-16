using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents;

[HarmonyPatch(typeof(SkirmishSettings_Beta), "LoadLanceConfiguratorData")]
public static class SkirmishSettings_Beta_LoadLanceConfiguratorData_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions
            .MethodReplacer(
                AccessTools.Method(typeof(DataManager), nameof(DataManager.CreateLoadRequest)),
                AccessTools.Method(typeof(BTLoadUtils), nameof(BTLoadUtils.CreateLoadRequest))
            );
    }
}