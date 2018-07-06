using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{

    [HarmonyPatch(typeof(MechLabLocationWidget), "RepairAll")]
    public static class MechLabLocationWidget_RepairAll_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
           .MethodReplacer(
                AccessTools.Method(typeof(MechLabLocationWidget), "OnRemoveItem"),
                AccessTools.Method(typeof(RemoveHelper), "OnRemoveItem")
            ).MethodReplacer(
                AccessTools.Method(typeof(MechLabPanel), "ForceItemDrop"),
                AccessTools.Method(typeof(RemoveHelper), "ForceItemDropRepair")
            );
        }
    }

    [HarmonyPatch(typeof(MechLabLocationWidget), "StripDestroyedComponents")]
    public static class MechLabLocationWidget_StripDestroyedComponents_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                AccessTools.Method(typeof(MechLabLocationWidget), "OnRemoveItem"),
                AccessTools.Method(typeof(RemoveHelper), "OnRemoveItem")
            ).MethodReplacer(
                AccessTools.Method(typeof(MechLabPanel), "ForceItemDrop"),
                AccessTools.Method(typeof(RemoveHelper), "ForceItemDropRepair")
            );
        }
    }
}