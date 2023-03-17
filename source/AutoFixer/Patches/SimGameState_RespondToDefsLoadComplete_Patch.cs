using System;
using System.Linq;
using BattleTech;

namespace CustomComponents;

[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.RespondToDefsLoadComplete))]
public static class SimGameState_RespondToDefsLoadComplete_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    public static void Prefix(SimGameState __instance)
    {
        try
        {
            var mechDefs = __instance.DataManager.MechDefs.Select(pair => pair.Value).ToList();
            MechDefProcessing.Instance.Process(mechDefs);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}