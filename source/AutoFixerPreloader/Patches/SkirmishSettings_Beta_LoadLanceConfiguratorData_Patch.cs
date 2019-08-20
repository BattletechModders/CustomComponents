using System.Collections.Generic;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(SkirmishSettings_Beta), "LoadLanceConfiguratorData")]
    public class SkirmishSettings_Beta_LoadLanceConfiguratorData_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AutoFixPreloader.ReplaceCreateLoadRequest(instructions);
        }
    }
}