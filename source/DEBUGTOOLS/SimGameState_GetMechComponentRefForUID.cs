using BattleTech;
using Harmony;
using System;

namespace CustomComponents;

[HarmonyPatch(typeof(SimGameState))]
[HarmonyPatch("GetMechComponentRefForUID")]

public static class SimGameState_GetMechComponentRefForUID
{
    public static void Prefix(MechDef mech, string simGameUID, string componentID, ComponentType componentType)
    {
        try
        {
            Log.ComponentInstall.Trace?.Log($"Prefix: Get ref for mech:{mech.Description.Id} suid:{simGameUID} cid:{componentID} type:{componentType}");
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log("PREFIX", e);
        }
    }

    public static void Postfix(MechDef mech, string simGameUID, string componentID, ComponentType componentType, MechComponentRef __result, SimGameState __instance)
    {
        try
        {
            Log.ComponentInstall.Trace?.Log($"Postfix: Get ref for mech:{mech.Description.Id} suid:{simGameUID} cid:{componentID} type:{componentType}");

            if (__result == null)
            {
                Log.ComponentInstall.Trace?.Log("- Component not found! start to check");
                Log.ComponentInstall.Trace?.Log("-- reserved");

                foreach (MechComponentRef r in __instance.WorkOrderComponents)
                {
                    Log.ComponentInstall.Trace?.Log($"--- id:{r.ComponentDefID} uid:{r.SimGameUID} type:{r.ComponentDefType}");
                }
            }
            else
            {
                Log.ComponentInstall.Trace?.Log("- Component found");
            }
            Log.ComponentInstall.Trace?.Log("- done");

        }
        catch (Exception e)
        {
            Log.Main.Error?.Log("Postfix", e);
        }
    }
}