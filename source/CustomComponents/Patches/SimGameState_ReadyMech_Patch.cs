﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BattleTech;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SimGameState), "ReadyMech")]
internal class SimGameState_ReadyMech_Patch
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

#if DEBUG
            DEBUGTOOLS.NEEDTOSHOW = true;
#endif
        state = __instance;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var constructor = typeof(MechDef).GetConstructor(new[] { typeof(ChassisDef), typeof(string), typeof(MechDef) });
        foreach (var codeInstruction in instructions)
        {
            if (codeInstruction.operand is ConstructorInfo operand && operand == constructor)
            {
                yield return new(OpCodes.Call,
                    ((Func<ChassisDef, string, MechDef, MechDef>)CreateMech).Method);
            }
            else
            {
                yield return codeInstruction;
            }
        }
    }

    public static MechDef CreateMech(ChassisDef chassis, string simuid, MechDef original)
    {
        Log.ClearInventory.Trace?.Log($"Createing Mech {chassis.Description.Id} - {original.Description.Id}");
        var result = new MechDef(chassis, simuid, original);
        DEBUGTOOLS.ShowInventory(result);
        DEBUGTOOLS.ShowInventory(original);
        result.SetInventory(DefaultHelper.ClearInventory(original, state));
        DEBUGTOOLS.ShowInventory(result);
        return result;
    }

}