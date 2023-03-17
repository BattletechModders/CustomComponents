using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.Rehydrate))]
public static class SimGameState_Rehydrate_Patch
{
    [HarmonyPostfix]
    public static void FixMechInMechbay(SimGameState __instance, StatCollection ___companyStats, Dictionary<int, MechDef> ___ActiveMechs, Dictionary<int, MechDef> ___ReadyingMechs)
    {
        try
        {
            var mechDefs = ___ActiveMechs.Values.Union(___ReadyingMechs.Values).ToList();
            MechDefProcessing.Instance.Process(mechDefs);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}