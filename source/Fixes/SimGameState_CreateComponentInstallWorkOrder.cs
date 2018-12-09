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
            string mechSimGameUID,
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
                MechDef mechByID = __instance.GetMechByID(mechSimGameUID);
#if CCDEBUG
                if (mechByID == null)
                    Control.Logger.LogDebug("-- no mech found!");
#endif
                if (mechByID != null && mechByID.Chassis.ChassisTags.Contains(Control.Settings.OmniTechFlag))
                {
#if CCDEBUG
                    if (mechByID == null)
                        Control.Logger.LogDebug("-- mech is omni!");
#endif

                    tr.Value = (Control.Settings.OmniTechCostBySize ? mechComponent.Def.InventorySize / 2 : 1) * Control.Settings.OmniTechInstallCost;
                }


                if (tr.Value == 0)
                    tr.Value = 1;
            }
        }
    }
}