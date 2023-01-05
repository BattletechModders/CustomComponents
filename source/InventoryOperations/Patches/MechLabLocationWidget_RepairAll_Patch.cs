using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;
using Harmony;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget), nameof(MechLabLocationWidget.RepairAll))]
public static class MechLabLocationWidget_RepairAll_Patch
{
    public static bool RepairAll(bool forceRepairStructure, bool validate, MechLabLocationWidget __instance)
    {
        var mechLab = MechLabHelper.CurrentMechLab;
        if (!mechLab.InMechLab || !mechLab.InSimGame)
            return false;

        if (forceRepairStructure || __instance.loadout.CurrentInternalStructure > 0f)
        {
            __instance.RepairStructure(validate);
        }

        var changes = new Queue<IChange>();
        var lheleper = mechLab.GetLocationHelper(__instance.loadout.Location);

        foreach (var item in lheleper.LocalInventory.Where(i =>
                     i.ComponentRef.DamageLevel != ComponentDamageLevel.Functional
                     && i.ComponentRef.DamageLevel != ComponentDamageLevel.Installing))
        {
            if (item.ComponentRef.IsFixed || item.ComponentRef.Flags<CCFlags>().AutoRepair)
            {
                item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
                item.RepairComponent(false);
            }
            else if (item.ComponentRef.DamageLevel != ComponentDamageLevel.Destroyed)
            {
                item.RepairComponent(true);
            }
            else
            {
                item.RepairComponent(true);
                changes.Add(new Change_Remove(item.ComponentRef.ComponentDefID, __instance.loadout.Location));
            }
        }

        var state = new InventoryOperationState(changes, mechLab.ActiveMech);
        state.DoChanges();
        state.ApplyMechlab();

        return false;
    }
}