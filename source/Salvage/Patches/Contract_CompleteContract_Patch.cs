using System;
using System.Collections.Generic;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(Contract), nameof(Contract.CompleteContract))]
    public static class Contract_CompleteContract_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .MethodReplacer(
                    AccessTools.Property(typeof(AbstractActor), nameof(AbstractActor.IsDead)).GetGetMethod(),
                    AccessTools.Method(typeof(Contract_CompleteContract_Patch), nameof(IsDead))
                )
                .MethodReplacer(
                    AccessTools.Property(typeof(Mech), nameof(Mech.IsDead)).GetGetMethod(),
                    AccessTools.Method(typeof(Contract_CompleteContract_Patch), nameof(IsDead))
                )
                .MethodReplacer(
                    AccessTools.Property(typeof(Turret), nameof(Turret.IsDead)).GetGetMethod(),
                    AccessTools.Method(typeof(Contract_CompleteContract_Patch), nameof(IsDead))
                )
                .MethodReplacer(
                    AccessTools.Property(typeof(Vehicle), nameof(Vehicle.IsDead)).GetGetMethod(),
                    AccessTools.Method(typeof(Contract_CompleteContract_Patch), nameof(IsDead))
                );
        }

        public static bool IsDead(this AbstractActor actor)
        {
            try
            {
                return IsDestroyed.IsActorDestroyed(actor);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }

            return false;
        }
    }
}