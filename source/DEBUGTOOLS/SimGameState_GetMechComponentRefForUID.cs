using BattleTech;
using Harmony;
using System;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("GetMechComponentRefForUID")]

    public static class SimGameState_GetMechComponentRefForUID
    {
        public static void Prefix(MechDef mech, string simGameUID, string componentID, ComponentType componentType)
        {
            try
            {
                Logging.Debug?.LogDebug(DType.ComponentInstall, $"Prefix: Get ref for mech:{mech.Description.Id} suid:{simGameUID} cid:{componentID} type:{componentType}");
            }
            catch (Exception e)
            {
                Logging.Error?.Log("PREFIX", e);
            }
        }

        public static void Postfix(MechDef mech, string simGameUID, string componentID, ComponentType componentType, MechComponentRef __result, SimGameState __instance)
        {
            try
            {
                Logging.Debug?.LogDebug(DType.ComponentInstall, $"Postfix: Get ref for mech:{mech.Description.Id} suid:{simGameUID} cid:{componentID} type:{componentType}");

                if (__result == null)
                {
                    Logging.Debug?.LogDebug(DType.ComponentInstall, "- Component not found! start to check");
                    Logging.Debug?.LogDebug(DType.ComponentInstall, "-- reserved");

                    foreach (MechComponentRef r in __instance.WorkOrderComponents)
                    {
                        Logging.Debug?.LogDebug(DType.ComponentInstall, $"--- id:{r.ComponentDefID} uid:{r.SimGameUID} type:{r.ComponentDefType}");
                    }
                }
                else
                {
                    Logging.Debug?.LogDebug(DType.ComponentInstall, "- Component found");
                }
                Logging.Debug?.LogDebug(DType.ComponentInstall, "- done");

            }
            catch (Exception e)
            {
                Logging.Error?.Log("Postfix", e);
            }
        }
    }
}
