using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "ReadyMech")]
    internal class SimGameState_ReadyMech_Patch
    {
        private static SimGameState state;

        public static void Prefix(SimGameState __instance)
        {
            state = __instance;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var constructor = typeof(MechDef).GetConstructor(new[] { typeof(ChassisDef), typeof(string), typeof(MechDef) });
            foreach (var codeInstruction in instructions)
            {
                if (codeInstruction.operand is ConstructorInfo operand && operand == constructor)
                {
                    yield return new CodeInstruction(OpCodes.Call,
                        ((Func<ChassisDef, string, MechDef, MechDef>)CreateMech).Method);
                }
                else
                    yield return codeInstruction;
            }
        }

        public static MechDef CreateMech(ChassisDef chassis, string simuid, MechDef original)
        {
            Control.Logger.LogDebug($"Createing Mech {chassis.Description.Id} - {original.Description.Id}");
            var result = new MechDef(chassis, simuid, original);
            result.SetInventory(DefaultHelper.ClearInventory(original, state));
            return result;
        }
    }
}