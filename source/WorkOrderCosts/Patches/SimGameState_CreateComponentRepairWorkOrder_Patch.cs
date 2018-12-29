using System;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "CreateComponentRepairWorkOrder")]
    public static class SimGameState_CreateComponentRepairWorkOrder_Patch
    {
        public static void Postfix(SimGameState __instance, MechComponentRef mechComponent, bool isOnMech, ref WorkOrderEntry_RepairComponent __result)
        {
            try
            {
                WorkOrderCostsHandler.Shared.ComponentRepairWorkOrder(mechComponent, isOnMech, __result);
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}
