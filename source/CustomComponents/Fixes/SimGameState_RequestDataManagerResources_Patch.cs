using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;

namespace CustomComponents.Fixes;

[HarmonyPatch(typeof(SimGameState), "RequestDataManagerResources")]
public static class SimGameState_RequestDataManagerResources_Patch
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