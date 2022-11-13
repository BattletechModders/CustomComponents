using BattleTech;
using Harmony;
using System;

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
            string mechSimGameUID,
            WorkOrderEntry_InstallComponent __result)
        {
            try
            {
                Logging.Debug?.LogDebug(DType.InstallCost, $"SimGameState_CreateComponentInstallWorkOrder: for {mechComponent.ComponentDefID}");
                Logging.Debug?.LogDebug(DType.InstallCost, $"-- from {previousLocation} to {newLocation}");
                Logging.Debug?.LogDebug(DType.InstallCost, $"-- order {__result?.GetType().ToString() ?? "null"}");

                if (__result == null)
                {
                    Logging.Debug?.LogDebug(DType.InstallCost, "-- No order");
                    return;

                }


                if (__result.DesiredLocation == ChassisLocations.None)
                {
                    __result.Cost = 0;
                }
                else
                {
                    MechDef mechByID = __instance.GetMechByID(mechSimGameUID);
#if CCDEBUG
                    if (mechByID == null)
                        Logging.Debug?.LogDebug(DType.InstallCost, "-- no mech found!");
#endif
                    if (mechByID != null && mechByID.Chassis.ChassisTags.Contains(Control.Settings.OmniTechFlag))
                    {
                        Logging.Debug?.LogDebug(DType.InstallCost, "-- mech is omni!");
                        __result.Cost = (Control.Settings.OmniTechCostBySize ? mechComponent.Def.InventorySize / 2 : 1) * Control.Settings.OmniTechInstallCost;
                    }


                    if (__result.Cost == 0)
                        __result.Cost = 1;
                }

            }
            catch (Exception e)
            {
                Logging.Error?.Log(e);
            }
        }
    }
}