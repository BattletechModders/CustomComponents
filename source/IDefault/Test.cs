using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
    public static class SimGameState_ML_InstallComponent
    {
        public static void Prefix(WorkOrderEntry_InstallComponent order)
        {
            Control.Logger.LogDebug($"install: {order.Description} - {order.MechComponentRef.SimGameUID} - {order.MechComponentRef.ComponentDefID} - {order.MechComponentRef.MountedLocation} - {order.MechComponentRef.Def}");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
    public static class SimGameState_GetMechComponentRefForUID
    {
        public static void Postfix(MechDef mech, string simGameUID, string componentID, ComponentType componentType, ComponentDamageLevel damageLevel, ChassisLocations desiredLocation, int hardpointSlot, ref bool itemWasFromInventory, MechComponentRef __result)
        {
            Control.Logger.LogDebug($"GetMechComponentRefForUID: {__result} - {simGameUID} - {componentID} - {itemWasFromInventory}");
        }
    }
}