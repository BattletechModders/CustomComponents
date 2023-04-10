using BattleTech;

namespace CustomComponents;

[HarmonyPatch(typeof(SimGameState))]
[HarmonyPatch(nameof(SimGameState.GetMechComponentRefForUID))]
public static class SimGameState_GetMechComponentRefForUID
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechDef mech, string simGameUID, string componentID, ComponentType componentType)
    {
        if (!__runOriginal)
        {
            return;
        }

        Log.ComponentInstall.Trace?.Log($"Prefix: Get ref for mech:{mech?.Description?.Id} suid:{simGameUID} cid:{componentID} type:{componentType}");
    }

    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(MechDef mech, string simGameUID, string componentID, ComponentType componentType, MechComponentRef __result, SimGameState __instance)
    {
        Log.ComponentInstall.Trace?.Log($"Postfix: Get ref for mech:{mech?.Description?.Id} suid:{simGameUID} cid:{componentID} type:{componentType}");

        if (__result == null)
        {
            Log.ComponentInstall.Trace?.Log("- Component not found! start to check");
            Log.ComponentInstall.Trace?.Log("-- reserved");

            foreach (var r in __instance.WorkOrderComponents)
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
}