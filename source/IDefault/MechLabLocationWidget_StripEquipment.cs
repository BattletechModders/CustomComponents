using System.Collections.Generic;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    internal static class MechLabLocationWidget_StripEquipment
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                AccessTools.Method(typeof(MechLabLocationWidget), "OnRemoveItem"),
                AccessTools.Method(typeof(RemoveHelper), "OnRemoveItemStrip")
            ).MethodReplacer(
                AccessTools.Method(typeof(MechLabPanel), "ForceItemDrop"),
                AccessTools.Method(typeof(RemoveHelper), "ForceItemDropStrip")
            );
        }
    }
}