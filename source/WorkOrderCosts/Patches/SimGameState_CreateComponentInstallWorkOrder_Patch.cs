using System;
using BattleTech;
using Harmony;

namespace CustomComponents;

[HarmonyPatch(typeof(SimGameState), "CreateComponentInstallWorkOrder")]
public static class SimGameState_CreateComponentInstallWorkOrder_Patch
{
    public static void Postfix(
        SimGameState __instance,
        string mechSimGameUID,
        MechComponentRef mechComponent,
        ChassisLocations newLocation,
        ChassisLocations previousLocation,
        WorkOrderEntry_InstallComponent __result
    )
    {
        try
        {
            var mechDef = __instance.GetMechByID(mechSimGameUID);
            WorkOrderCostsHandler.Shared.ComponentInstallWorkOrder(mechDef, mechComponent, newLocation, __result);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}