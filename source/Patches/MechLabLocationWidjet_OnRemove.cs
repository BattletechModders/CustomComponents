using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BattleTech;
using BattleTech.UI;
using Harmony;

using System.IO;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "OnRemoveItem")]
    public static class MechLabLocationWidjet_OnRemove
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int i = 0;

            foreach (var codeInstruction in instructions)
            {
               
                if (i >=3 && i <=8)
                    yield return new CodeInstruction(OpCodes.Nop);
                else
                    yield return codeInstruction;

                i++;
            }

        }
    }
}