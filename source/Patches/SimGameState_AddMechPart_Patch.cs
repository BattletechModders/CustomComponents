using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BattleTech;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SimGameState), "AddMechPart")]
public static class SimGameState_AddMechPart_Patch
{
    private static SimGameState state;

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, SimGameState __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        state = __instance;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var constructor = typeof(MechDef).GetConstructor(new[] { typeof(MechDef), typeof(string), typeof(bool) });
        foreach (var codeInstruction in instructions)
        {
            if (codeInstruction.operand is ConstructorInfo operand && operand == constructor)
            {
                yield return new(OpCodes.Call,
                    ((Func<MechDef, string, bool, MechDef>)CreateMech).Method);
            }
            else
                yield return codeInstruction;
        }
    }

    private static MechDef CreateMech(MechDef mechDef, string simguid, bool need_equip)
    {
        Log.ClearInventory.Trace?.Log($"Create mech from parts need_equip:{need_equip}");

        var result = new MechDef(mechDef, simguid);
        if (!need_equip)
        {
            result.SetInventory(DefaultHelper.ClearInventory(mechDef, state));
        }

        return result;
    }
}