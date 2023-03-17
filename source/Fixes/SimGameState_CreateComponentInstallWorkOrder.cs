using BattleTech;

namespace CustomComponents.Fixes;

[HarmonyPatch(typeof(SimGameState), "CreateComponentInstallWorkOrder")]
public static class SimGameState_CreateComponentInstallWorkOrder
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(
        SimGameState __instance,
        MechComponentRef mechComponent,
        ChassisLocations newLocation,
        ChassisLocations previousLocation,
        string mechSimGameUID,
        WorkOrderEntry_InstallComponent __result)
    {
        Log.InstallCost.Trace?.Log($"SimGameState_CreateComponentInstallWorkOrder: for {mechComponent.ComponentDefID}");
        Log.InstallCost.Trace?.Log($"-- from {previousLocation} to {newLocation}");
        Log.InstallCost.Trace?.Log($"-- order {__result?.GetType().ToString() ?? "null"}");

        if (__result == null)
        {
            Log.InstallCost.Trace?.Log("-- No order");
            return;

        }


        if (__result.DesiredLocation == ChassisLocations.None)
        {
            __result.Cost = 0;
        }
        else
        {
            var mechByID = __instance.GetMechByID(mechSimGameUID);
#if DEBUG
                    if (mechByID == null)
                        Log.InstallCost.Trace?.Log("-- no mech found!");
#endif
            if (mechByID != null && mechByID.Chassis.ChassisTags.Contains(Control.Settings.OmniTechFlag))
            {
                Log.InstallCost.Trace?.Log("-- mech is omni!");
                __result.Cost = (Control.Settings.OmniTechCostBySize ? mechComponent.Def.InventorySize / 2 : 1) * Control.Settings.OmniTechInstallCost;
            }


            if (__result.Cost == 0)
                __result.Cost = 1;
        }
    }
}