using System;
using System.Collections.Generic;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.ML_InstallComponent))]
public static class SimGameState_ML_InstallComponent_Patch
{
    public static void Postfix(WorkOrderEntry_InstallComponent order, SimGameState __instance)
    {
        try
        {
            Log.ComponentInstall.Trace?.Log($"ML_InstallComponent {order.MechComponentRef.ComponentDefID} - {order.MechComponentRef.Def == null}");
            if (!order.IsMechLabComplete)
            {
                return;
            }

            var mech = __instance.GetMechByID(order.MechID);
            if (mech == null)
            {
                return;
            }

            var changes = new Queue<IChange>();
            if (order.PreviousLocation != ChassisLocations.None)
            {
                changes.Enqueue(new Change_Remove(order.MechComponentRef, order.PreviousLocation, true));
            }

            if (order.DesiredLocation != ChassisLocations.None)
            {
                changes.Enqueue(new Change_Add(order.MechComponentRef, order.DesiredLocation, true));
            }

            var state = new InventoryOperationState(changes, mech);
            state.DoChanges();
            state.ApplyInventory();

            Log.ComponentInstall.Trace?.Log("ML_InstallComponent complete");
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }

}