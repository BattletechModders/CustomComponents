using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget), nameof(MechLabLocationWidget.RepairStructure))]
public static class MechLabLocationWidget_RepairStructure_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance, ref Queue<IChange> __state)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (!__instance.mechLab.IsSimGame)
        {
            return;
        }

        __state = new Queue<IChange>();
        Log.InventoryOperations.Trace?.Log($"RepairStructure called for {__instance.mechLab.activeMechDef.Description.Id} at {__instance.loadout.Location}");
        foreach (var item in __instance.localInventory)
        {
            var cRef = item.ComponentRef;
            Log.InventoryOperations.Trace?.Log($"- checking {cRef.ComponentDefID} {cRef.DamageLevel}");

            if (cRef.DamageLevel is ComponentDamageLevel.Functional or ComponentDamageLevel.Installing)
            {
                Log.InventoryOperations.Trace?.Log("-- skip");
                continue;
            }

            // see AutoRepairFixedEquipment
            if (cRef.IsFixed || cRef.Def.CCFlags().AutoRepair)
            {
                Log.InventoryOperations.Trace?.Log("-- repair no validation - is fixed or auto repair");
                cRef.DamageLevel = ComponentDamageLevel.Penalized;
                item.RepairComponent(false);
            }
            else if (cRef.DamageLevel != ComponentDamageLevel.Destroyed)
            {
                Log.InventoryOperations.Trace?.Log("-- repair with validation");
                item.RepairComponent(true);
            }
            else
            {
                Log.InventoryOperations.Trace?.Log("-- remove");
                // will be removed between Prefix and Postfix by then original method
                __state.Enqueue(new Change_Remove(cRef, __instance.loadout.Location, true));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(MechLabLocationWidget __instance, ref Queue<IChange> __state)
    {
        if (!__instance.mechLab.IsSimGame)
        {
            return;
        }

        if (__state is { Count: > 0 })
        {
            var state = new InventoryOperationState(__state, __instance.mechLab.activeMechDef);
            state.DoChanges();
            state.ApplyMechlab();
        }
    }
}