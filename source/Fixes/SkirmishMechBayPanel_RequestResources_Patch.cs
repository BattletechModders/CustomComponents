using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SkirmishMechBayPanel), "RequestResources")]
    public static class SkirmishMechBayPanel_RequestResources_Patch
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
}