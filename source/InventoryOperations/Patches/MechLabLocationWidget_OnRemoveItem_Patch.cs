using System.Collections.Generic;
using System.Reflection.Emit;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget), nameof(MechLabLocationWidget.OnRemoveItem))]
public static class MechLabLocationWidget_OnRemoveItem_Patch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var i = 0;

        foreach (var codeInstruction in instructions)
        {
               
            if (i >=3 && i <=8)
                yield return new(OpCodes.Nop);
            else
                yield return codeInstruction;

            i++;
        }

    }
}