using System.Collections.Generic;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(SkirmishMechBayPanel), "RequestResources")]
    public class SkirmishMechBayPanel_RequestResources_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return AutoFixPreloader.ReplaceCreateLoadRequest(instructions);
        }
    }
}