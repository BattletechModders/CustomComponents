using BattleTech;
using Harmony;

namespace CustomComponents.Fixes
{
    [HarmonyPatch(typeof(SimGameState), "CreateComponentInstallWorkOrder")]
    public static class SimGameState_CreateComponentInstallWorkOrder
    {
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void FixCost(
            SimGameState __instance,
            MechComponentRef mechComponent,
            ChassisLocations newLocation,
            ChassisLocations previousLocation,
            WorkOrderEntry_InstallComponent __result)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"SimGameState_CreateComponentInstallWorkOrder: for {mechComponent.ComponentDefID}");
            Control.Logger.LogDebug($"-- from {previousLocation} to {newLocation}");
            Control.Logger.LogDebug($"-- order {__result?.GetType().ToString() ?? "null"}");

#endif
            if (__result == null)
            {
                Control.Logger.LogError("-- No order");
                return;

            }


            if (__result.DesiredLocation == ChassisLocations.None)
            {
                var tr = Traverse.Create(__result);
                tr.Field<int>("Cost").Value = 0;
            }
            else
            {
                var tr = Traverse.Create(__result).Field<int>("Cost");

                if (tr == null)
                {
                    Control.Logger.LogDebug("SimGameState_CreateComponentInstallWorkOrder: traverce not created!");
                    return;

                }
                if (tr.Value == 0)
                    tr.Value = 1;
            }
        }
    }
}