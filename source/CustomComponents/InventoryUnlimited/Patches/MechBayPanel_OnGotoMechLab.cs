#nullable disable
// ReSharper disable InconsistentNaming
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.InventoryUnlimited.Patches;

[HarmonyPatch(typeof(MechBayPanel), nameof(MechBayPanel.OnGotoMechLab))]
internal static class MechBayPanel_OnGotoMechLab
{
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions
            .MethodReplacer(
                AccessTools.Method(typeof(SimGameState), nameof(SimGameState.GetAllInventoryItemDefs)),
                AccessTools.Method(typeof(MechBayPanel_OnGotoMechLab), nameof(GetAllInventoryItemDefs))
            );
    }

    internal static List<MechComponentRef> GetAllInventoryItemDefs(this SimGameState __instance)
    {
        var list = __instance.GetAllInventoryItemDefs();
        foreach (var componentDef in UnlimitedFeature.UnlimitedItemDefs)
        {
            var mechComponentRef = new MechComponentRef(componentDef.Description.Id, __instance.GenerateSimGameUID(), componentDef.ComponentType, ChassisLocations.None);
            mechComponentRef.SetComponentDef(componentDef);
            list.Add(mechComponentRef);
        }
        return list;
    }
}