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
                Control.LogDebug(DType.InstallCost, $"SimGameState_CreateComponentInstallWorkOrder: for {mechComponent.ComponentDefID}");
                Control.LogDebug(DType.InstallCost, $"-- from {previousLocation} to {newLocation}");
                Control.LogDebug(DType.InstallCost, $"-- order {__result?.GetType().ToString() ?? "null"}");

                if (__result == null)
                {
                    Control.LogDebug(DType.InstallCost, "-- No order");
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
                        Control.LogDebug(DType.InstallCost, "SimGameState_CreateComponentInstallWorkOrder: traverce not created!");
                        return;

                    }
                    MechDef mechByID = __instance.GetMechByID(mechSimGameUID);
#if CCDEBUG
                    if (mechByID == null)
                        Control.LogDebug(DType.InstallCost, "-- no mech found!");
#endif
                    if (mechByID != null && mechByID.Chassis.ChassisTags.Contains(Control.Settings.OmniTechFlag))
                    {
                        Control.LogDebug(DType.InstallCost, "-- mech is omni!");
                        tr.Value = (Control.Settings.OmniTechCostBySize ? mechComponent.Def.InventorySize / 2 : 1) * Control.Settings.OmniTechInstallCost;
                    }


                    if (tr.Value == 0)
                        tr.Value = 1;
                }

            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}